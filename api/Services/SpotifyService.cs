using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace MediaMatch.Services
{
    public class SpotifyService
    {
        private readonly HttpClient _http;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private string? _accessToken;
        private DateTime _tokenExpiresAt;

        public SpotifyService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _clientId = config["Spotify:ClientId"] ?? Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID") ?? "";
            _clientSecret = config["Spotify:ClientSecret"] ?? Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET") ?? "";
        }

        private async Task EnsureAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiresAt)
                return;

            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            using var req = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
            req.Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") });

            using var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);
            _accessToken = doc.RootElement.GetProperty("access_token").GetString();
            var expiresIn = doc.RootElement.GetProperty("expires_in").GetInt32();
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 30);
        }

        public async Task<SpotifyAlbum?> SearchAlbumAsync(string title, int? year, string? composer)
        {
            await EnsureAccessTokenAsync();

            var queryParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(title)) queryParts.Add($"album:\"{title}\"");
            if (!string.IsNullOrWhiteSpace(composer)) queryParts.Add($"artist:\"{composer}\"");
            queryParts.Add("(soundtrack OR \"original score\" OR \"original motion picture soundtrack\" OR \"original series soundtrack\" OR OST)");
            if (year.HasValue) queryParts.Add($"year:{year.Value - 1}-{year.Value + 1}");

            var q = string.Join(" ", queryParts);
            var url = $"https://api.spotify.com/v1/search?type=album&limit=10&market=BR&q={Uri.EscapeDataString(q)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            using var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<SpotifySearchResponse>(json, JsonOptions);
            var albums = data?.albums?.items ?? new List<SpotifyAlbum>();

            var preferred = albums
                .OrderByDescending(a => ScoreAlbumMatch(a, title, composer, year))
                .FirstOrDefault();

            if (preferred == null) return null;

            var strictTitle = IsTitleMatchStrict(preferred.name, title);
            var composerOk = !string.IsNullOrWhiteSpace(composer) && (preferred.artists?.Any(a => string.Equals(a.name, composer, StringComparison.OrdinalIgnoreCase)) ?? false);
            var yearOk = IsYearClose(preferred.release_date, year);

            if (!strictTitle && !composerOk && !yearOk)
                return null;

            return preferred;
        }

        public async Task<SpotifyAlbum?> SearchAlbumBasicAsync(string title, int? year)
        {
            await EnsureAccessTokenAsync();
            var queryParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(title)) queryParts.Add($"album:\"{title}\"");
            if (year.HasValue) queryParts.Add($"year:{year.Value - 1}-{year.Value + 1}");
            var q = string.Join(" ", queryParts);
            var url = $"https://api.spotify.com/v1/search?type=album&limit=10&market=BR&q={Uri.EscapeDataString(q)}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            using var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<SpotifySearchResponse>(json, JsonOptions);
            var albums = data?.albums?.items ?? new List<SpotifyAlbum>();
            var preferred = albums.OrderByDescending(a => ScoreAlbumMatch(a, title, null, year)).FirstOrDefault();

            if (preferred == null) return null;
            var strictTitle = IsTitleMatchStrict(preferred.name, title);
            var yearOk = IsYearClose(preferred.release_date, year);
            if (!strictTitle && !yearOk)
                return null;

            return preferred;
        }

        public async Task<List<SpotifyTrack>> GetAlbumTracksAsync(string albumId)
        {
            await EnsureAccessTokenAsync();
            var url = $"https://api.spotify.com/v1/albums/{albumId}/tracks?market=BR&limit=50";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            using var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<SpotifyTracksResponse>(json, JsonOptions);
            return data?.items ?? new List<SpotifyTrack>();
        }

        public async Task<SpotifyAlbum?> SearchAlbumLooseAsync(string query)
        {
            await EnsureAccessTokenAsync();
            var url = $"https://api.spotify.com/v1/search?type=album&limit=10&market=BR&q={Uri.EscapeDataString(query)}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            using var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<SpotifySearchResponse>(json, JsonOptions);
            var albums = data?.albums?.items ?? new List<SpotifyAlbum>();
            var preferred = albums.OrderByDescending(a => ScoreAlbumMatch(a, query, null, null)).FirstOrDefault();
            return preferred;
        }

        private static string GetBaseTitle(string name)
        {
            var idx = name.IndexOf(" (", StringComparison.Ordinal);
            if (idx > 0) return name.Substring(0, idx);
            return name;
        }

        private static bool IsTitleMatchStrict(string albumName, string title)
        {
            var a = GetBaseTitle(albumName).Trim();
            var t = title.Trim();
            return string.Equals(a, t, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsYearClose(string releaseDate, int? year)
        {
            if (!year.HasValue) return false;
            if (string.IsNullOrWhiteSpace(releaseDate) || releaseDate.Length < 4) return false;
            if (!int.TryParse(releaseDate.Substring(0, 4), out var y)) return false;
            var diff = Math.Abs(y - year.Value);
            return diff <= 1;
        }

        private static int ScoreAlbumMatch(SpotifyAlbum album, string title, string? composer, int? year)
        {
            var name = album.name ?? string.Empty;
            var baseTitle = GetBaseTitle(name);
            var score = 0;

            if (IsTitleMatchStrict(name, title)) score += 10;
            else
            {
                var pattern = $"\\b{Regex.Escape(title)}\\b";
                if (Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase)) score += 4;
            }

            var lower = name.ToLowerInvariant();
            if (lower.Contains("original motion picture soundtrack")) score += 3;
            if (lower.Contains("original score")) score += 2;
            if (lower.Contains("original series soundtrack")) score += 2;
            if (lower.Contains("soundtrack")) score += 1;

            if (!string.IsNullOrWhiteSpace(composer) && (album.artists?.Any(a => string.Equals(a.name, composer, StringComparison.OrdinalIgnoreCase)) ?? false))
                score += 6;

            if (IsYearClose(album.release_date, year)) score += 3;

            if (!string.Equals(GetBaseTitle(name), title, StringComparison.OrdinalIgnoreCase))
                score -= 1;

            return score;
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public class SpotifySearchResponse
        {
            public Albums albums { get; set; }
        }

        public class Albums
        {
            public List<SpotifyAlbum> items { get; set; }
        }

        public class SpotifyAlbum
        {
            public string id { get; set; }
            public string name { get; set; }
            public string release_date { get; set; }
            public ExternalUrls external_urls { get; set; }
            public List<SpotifyArtist> artists { get; set; }
        }

        public class SpotifyArtist
        {
            public string name { get; set; }
        }

        public class ExternalUrls
        {
            public string spotify { get; set; }
        }

        public class SpotifyTracksResponse
        {
            public List<SpotifyTrack> items { get; set; }
        }

        public class SpotifyTrack
        {
            public string id { get; set; }
            public string name { get; set; }
            public int duration_ms { get; set; }
            public List<SpotifyArtist> artists { get; set; }
            public ExternalUrls external_urls { get; set; }
            public string preview_url { get; set; }
        }
    }
}