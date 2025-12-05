using MediaMatch.Data; 
using MediaMatch.Middleware;
using MediaMatch.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MediaMatch.Security;
using System.Security.Claims;
using MediaMatch.Models;

LoadEnv(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<MediaMatchContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar serialização JSON para evitar referências circulares
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<TmdbService>();
builder.Services.AddHttpClient<SpotifyService>();
builder.Services.AddScoped<ClubService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<MediaListService>();
builder.Services.AddScoped<MediaListItemService>();
builder.Services.AddScoped<SoundtrackAggregator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("rolesLimiter", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 10;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validated for user: {User}", context.Principal?.Identity?.Name ?? "Unknown");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.Requirements.Add(new AdminRoleRequirement()));
});

builder.Services.AddScoped<IAuthorizationHandler, AdminRoleHandler>();

builder.Services.AddHttpClient<AudioDbApiService>(c =>
{
    c.BaseAddress = new Uri("https://theaudiodb.com/api/v1/json/2/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware de tratamento global de exceções (DEVE VIR ANTES de outros middlewares)
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

// Servir arquivos estáticos (imagens de upload)
app.UseStaticFiles();

app.UseCors("AllowAngularDev");

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

await EnsureSeedDataAsync(app.Services);

app.Run();
// temporario, uso de ENV para configurar as chaves da API do spotiy 
// - lembrar de colocar aquelas das outras api aqui tb
static void LoadEnv(string path)
{
    if (!File.Exists(path)) return;
    foreach (var line in File.ReadAllLines(path))
    {
        var s = line.Trim();
        if (string.IsNullOrEmpty(s)) continue;
        if (s.StartsWith("#")) continue;
        var idx = s.IndexOf('=');
        if (idx <= 0) continue;
        var key = s.Substring(0, idx).Trim();
        var value = s.Substring(idx + 1).Trim();
        if ((value.StartsWith("\"") && value.EndsWith("\"")) || (value.StartsWith("'") && value.EndsWith("'")))
        {
            value = value.Substring(1, value.Length - 2);
        }
        Environment.SetEnvironmentVariable(key, value);
    }
}

static async Task EnsureSeedDataAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MediaMatchContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("DatabaseSeeder");

    try
    {
        await context.Database.MigrateAsync();

        var roleNames = new[] { "admin", "user" };
        var rolesCreated = false;

        foreach (var roleName in roleNames)
        {
            var normalized = roleName.ToLower();
            if (!await context.Roles.AnyAsync(r => r.Name.ToLower() == normalized))
            {
                context.Roles.Add(new Role { Name = normalized });
                rolesCreated = true;
            }
        }

        if (rolesCreated)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Default roles ensured.");
        }

        var adminEmail = configuration["SeedAdmin:Email"] ?? Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL");
        var adminPassword = configuration["SeedAdmin:Password"] ?? Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD");
        var adminUserName = configuration["SeedAdmin:UserName"] ?? Environment.GetEnvironmentVariable("SEED_ADMIN_USERNAME") ?? "admin";

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("Seed admin credentials missing. Skipping admin user creation.");
            return;
        }

        var adminRole = await context.Roles.FirstAsync(r => r.Name.ToLower() == "admin");

        var adminUser = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminUserName,
                Email = adminEmail,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            logger.LogInformation("Seed admin user created with email {Email}.", adminEmail);
        }

        var hasAdminRole = adminUser.UserRoles.Any(ur => ur.RoleId == adminRole.Id);
        if (!hasAdminRole)
        {
            context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
            await context.SaveChangesAsync();
            logger.LogInformation("Seed admin role ensured for {Email}.", adminEmail);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error while seeding database.");
    }
}
