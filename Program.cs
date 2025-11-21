using MediaMatch.Data; // Importante: Namespace onde est� o seu DbContext
using MediaMatch.Services;
using Microsoft.EntityFrameworkCore;

LoadEnv(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
var builder = WebApplication.CreateBuilder(args);

// 1. Recupera a string de conex�o do arquivo appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. === CONFIGURA��O DO DBCONTEXT ===
// Injeta o MediaMatchContext no container de servi�os usando SQL Server
builder.Services.AddDbContext<MediaMatchContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
