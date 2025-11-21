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
            http.BaseAddress = new Uri("https://theaudiodb.com/api/v1/json/2/");
            _http = http;
        }

        // ======================================================
        // BUSCAR ARTISTA
        // ======================================================
        public async Task<AudioDbArtistResponse?> SearchArtistAsync(string name)
        {
            var json = await _http.GetStringAsync($"search.php?s={name}");
            return JsonSerializer.Deserialize<AudioDbArtistResponse>(json, _jsonOptions);
        }

        // ======================================================
        // BUSCAR ÁLBUNS DO ARTISTA
        // ======================================================
        public async Task<AudioDbAlbumResponse?> GetAlbumsAsync(int artistId)
        {
            var json = await _http.GetStringAsync($"album.php?i={artistId}");
            return JsonSerializer.Deserialize<AudioDbAlbumResponse>(json, _jsonOptions);
        }

        // ======================================================
        // BUSCAR FAIXAS DO ÁLBUM
        // ======================================================
        public async Task<AudioDbTrackResponse?> GetTracksAsync(int albumId)
        {
            var json = await _http.GetStringAsync($"track.php?m={albumId}");
            return JsonSerializer.Deserialize<AudioDbTrackResponse>(json, _jsonOptions);
        }
    }
}
