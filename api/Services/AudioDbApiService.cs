using MediaMatch.DTO.TADB;
using System.Text.Json;

namespace MediaMatch.Services
{
    public class AudioDbApiService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AudioDbApiService(HttpClient http)
        {
            // Nota: "2" é a chave de testes. Se tiveres uma chave paga, altera aqui ou no appsettings
            http.BaseAddress = new Uri("https://theaudiodb.com/api/v1/json/2/");
            _http = http;
        }

        // ======================================================
        // ARTISTA
        // ======================================================
        public async Task<AudioDbArtistResponse?> SearchArtistAsync(string name)
        {
            // 1. IMPORTANTE: Usar Uri.EscapeDataString para nomes com espaço (ex: "Pink Floyd")
            var encodedName = Uri.EscapeDataString(name);
            
            try 
            {
                var json = await _http.GetStringAsync($"search.php?s={encodedName}");

                // 2. A API retorna "{\"artists\":null}" quando não acha nada.
                // Precisamos tratar isso antes de deserializar ou garantir que o DTO aceite nulo.
                if (string.IsNullOrWhiteSpace(json) || json.Contains("\"artists\":null")) 
                {
                    return null; 
                }

                return JsonSerializer.Deserialize<AudioDbArtistResponse>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                // Logar erro se necessário
                Console.WriteLine($"Erro ao buscar artista: {ex.Message}");
                return null;
            }
        }

        // ======================================================
        // ÁLBUNS
        // ======================================================
        public async Task<AudioDbAlbumResponse?> GetAlbumsAsync(int artistId)
        {
            var json = await _http.GetStringAsync($"album.php?i={artistId}");
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonSerializer.Deserialize<AudioDbAlbumResponse>(json, _jsonOptions);
        }

        public async Task<AudioDbAlbumResponse?> GetAlbumByIdAsync(int albumId)
        {
            var json = await _http.GetStringAsync($"album.php?m={albumId}");
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonSerializer.Deserialize<AudioDbAlbumResponse>(json, _jsonOptions);
        }

        // NOVO MÉTODO: Busca Álbum por Nome (Requer nome do Artista)
        public async Task<AudioDbAlbumResponse?> SearchAlbumByNameAsync(string artistName, string albumName)
        {
            var url = $"searchalbum.php?s={Uri.EscapeDataString(artistName)}&a={Uri.EscapeDataString(albumName)}";
            var json = await _http.GetStringAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json.Contains("\"album\":null")) return null;
            return JsonSerializer.Deserialize<AudioDbAlbumResponse>(json, _jsonOptions);
        }

        // ======================================================
        // FAIXAS (TRACKS)
        // ======================================================
        public async Task<AudioDbTrackResponse?> GetTracksAsync(int albumId)
        {
            var json = await _http.GetStringAsync($"track.php?m={albumId}");
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonSerializer.Deserialize<AudioDbTrackResponse>(json, _jsonOptions);
        }

        public async Task<AudioDbTrackResponse?> SearchTrackAsync(string artist, string track)
        {
            var url = $"searchtrack.php?s={Uri.EscapeDataString(artist)}&t={Uri.EscapeDataString(track)}";
            var json = await _http.GetStringAsync(url);
            
            if (string.IsNullOrWhiteSpace(json) || json.Contains("\"track\":null")) return null;
            
            try
            {
                return JsonSerializer.Deserialize<AudioDbTrackResponse>(json, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<AudioDbTrackResponse?> SearchTrackByNameAsync(string track)
        {
            var url = $"searchtrack.php?t={Uri.EscapeDataString(track)}";
            var json = await _http.GetStringAsync(url);
            
            if (string.IsNullOrWhiteSpace(json) || json.Contains("\"track\":null")) return null;

            try
            {
                return JsonSerializer.Deserialize<AudioDbTrackResponse>(json, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }
    }
}