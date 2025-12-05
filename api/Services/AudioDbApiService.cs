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
        // 1. ARTISTAS
        // ======================================================
        public async Task<AudioDbArtistResponse?> SearchArtistAsync(string name)
        {
            var encodedName = Uri.EscapeDataString(name);
            try 
            {
                var json = await _http.GetStringAsync($"search.php?s={encodedName}");
                if (string.IsNullOrWhiteSpace(json) || json.Contains("\"artists\":null")) return null;
                return JsonSerializer.Deserialize<AudioDbArtistResponse>(json, _jsonOptions);
            }
            catch { return null; }
        }

        // ======================================================
        // 2. ÁLBUNS
        // ======================================================
        
        // Busca TODOS os álbuns pelo ID do Artista (album.php?i=...)
        public async Task<AudioDbAlbumResponse?> GetAlbumsAsync(int artistId)
        {
            var json = await _http.GetStringAsync($"album.php?i={artistId}");
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonSerializer.Deserialize<AudioDbAlbumResponse>(json, _jsonOptions);
        }

        // NOVO: Busca TODOS os álbuns pelo Nome do Artista (searchalbum.php?s=...)
        public async Task<AudioDbAlbumResponse?> GetAlbumsByArtistNameAsync(string artistName)
        {
            var encodedName = Uri.EscapeDataString(artistName);
            var json = await _http.GetStringAsync($"searchalbum.php?s={encodedName}");
            
            if (string.IsNullOrWhiteSpace(json) || json.Contains("\"album\":null")) return null;
            
            return JsonSerializer.Deserialize<AudioDbAlbumResponse>(json, _jsonOptions);
        }

        // Busca um álbum Específico pelo ID (album.php?m=...)
        // *** ESTE É O MÉTODO QUE ESTAVA FALTANDO ***
        public async Task<AudioDbAlbumResponse?> GetAlbumByIdAsync(int albumId)
        {
            var json = await _http.GetStringAsync($"album.php?m={albumId}");
            if (string.IsNullOrWhiteSpace(json)) return null;
            return JsonSerializer.Deserialize<AudioDbAlbumResponse>(json, _jsonOptions);
        }

        // Busca um álbum Específico pelo Nome (searchalbum.php?s=...&a=...)
        public async Task<AudioDbAlbumResponse?> SearchAlbumByNameAsync(string artistName, string albumName)
        {
            var url = $"searchalbum.php?s={Uri.EscapeDataString(artistName)}&a={Uri.EscapeDataString(albumName)}";
            var json = await _http.GetStringAsync(url);
            
            if (string.IsNullOrWhiteSpace(json) || json.Contains("\"album\":null")) return null;
            
            return JsonSerializer.Deserialize<AudioDbAlbumResponse>(json, _jsonOptions);
        }

        // ======================================================
        // 3. FAIXAS (TRACKS)
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
            
            try { return JsonSerializer.Deserialize<AudioDbTrackResponse>(json, _jsonOptions); } catch { return null; }
        }

        public async Task<AudioDbTrackResponse?> SearchTrackByNameAsync(string track)
        {
            var url = $"searchtrack.php?t={Uri.EscapeDataString(track)}";
            var json = await _http.GetStringAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json.Contains("\"track\":null")) return null;
            try { return JsonSerializer.Deserialize<AudioDbTrackResponse>(json, _jsonOptions); } catch { return null; }
        }

        // ======================================================
        // 4. DISCOGRAFIA
        // ======================================================
        public async Task<AudioDbAlbumResponse?> GetDiscographyByNameAsync(string artistName)
        {
            var encodedName = Uri.EscapeDataString(artistName);
            var json = await _http.GetStringAsync($"discography.php?s={encodedName}");

            if (string.IsNullOrWhiteSpace(json) || json.Contains("\"album\":null")) return null;

            return JsonSerializer.Deserialize<AudioDbAlbumResponse>(json, _jsonOptions);
        }

        public async Task<AudioDbAlbumResponse?> GetDiscographyByMbIdAsync(string mbId)
        {
            var encodedId = Uri.EscapeDataString(mbId);
            var json = await _http.GetStringAsync($"discography-mb.php?s={encodedId}");

            if (string.IsNullOrWhiteSpace(json) || json.Contains("\"album\":null")) return null;

            return JsonSerializer.Deserialize<AudioDbAlbumResponse>(json, _jsonOptions);
        }
        
         // ======================================================
        // DISCOVERY / POPULARES (Necessário para corrigir o 404)
        // ======================================================

        public async Task<List<AudioDbTrackDto>> GetMostLovedTracksAsync()
        {
            try
            {
                // format=track retorna as 50 músicas mais votadas
                var json = await _http.GetStringAsync("mostloved.php?format=track");
                
                if (string.IsNullOrWhiteSpace(json)) return new List<AudioDbTrackDto>();

                // Precisa do DTO AudioDbLovedResponse<T> criado anteriormente
                var result = JsonSerializer.Deserialize<AudioDbLovedResponse<AudioDbTrackDto>>(json, _jsonOptions);
                return result?.loved ?? new List<AudioDbTrackDto>();
            }
            catch
            {
                return new List<AudioDbTrackDto>();
            }
        }

        public async Task<List<AudioDbAlbumDto>> GetMostLovedAlbumsAsync()
        {
            try
            {
                var json = await _http.GetStringAsync("mostloved.php?format=album");
                if (string.IsNullOrWhiteSpace(json)) return new List<AudioDbAlbumDto>();

                var result = JsonSerializer.Deserialize<AudioDbLovedResponse<AudioDbAlbumDto>>(json, _jsonOptions);
                return result?.loved ?? new List<AudioDbAlbumDto>();
            }
            catch
            {
                return new List<AudioDbAlbumDto>();
            }
        }
    }
}