using MediaMatch.Data; 
using MediaMatch.Services;
using Microsoft.EntityFrameworkCore;

LoadEnv(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<MediaMatchContext>(options =>
    options.UseSqlServer(connectionString));


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddScoped<MediaListService>();
builder.Services.AddScoped<MediaListItemService>();
builder.Services.AddScoped<SoundtrackAggregator>();


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

app.UseHttpsRedirection();

app.UseCors("AllowAngularDev");

app.UseAuthorization();

app.MapControllers();

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
