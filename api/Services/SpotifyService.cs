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
            var url = $"https://api.spotify.com/v1/search?type=album&limit=20&market=BR&q={Uri.EscapeDataString(q)}";

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

            var score = ScoreAlbumMatch(preferred, title, composer, year);
            if (score >= 7)
                return preferred;

            return null;
        }

        public async Task<SpotifyAlbum?> SearchAlbumBasicAsync(string title, int? year)
        {
            await EnsureAccessTokenAsync();
            var queryParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(title)) queryParts.Add($"album:\"{title}\"");
            if (year.HasValue) queryParts.Add($"year:{year.Value - 1}-{year.Value + 1}");
            var q = string.Join(" ", queryParts);
            var url = $"https://api.spotify.com/v1/search?type=album&limit=20&market=BR&q={Uri.EscapeDataString(q)}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            using var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<SpotifySearchResponse>(json, JsonOptions);
            var albums = data?.albums?.items ?? new List<SpotifyAlbum>();
            var preferred = albums.OrderByDescending(a => ScoreAlbumMatch(a, title, null, year)).FirstOrDefault();

            if (preferred == null) return null;
            var score = ScoreAlbumMatch(preferred, title, null, year);
            // Aceita se tiver uma pontuação razoável
            if (score >= 5)
                return preferred;

            return null;
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
            var url = $"https://api.spotify.com/v1/search?type=album&limit=20&market=BR&q={Uri.EscapeDataString(query)}";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            using var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<SpotifySearchResponse>(json, JsonOptions);
            var albums = data?.albums?.items ?? new List<SpotifyAlbum>();
            
            // Extrai o título base da query (remove "soundtrack", "OST", etc)
            var baseQuery = query
                .Replace(" soundtrack", "", StringComparison.OrdinalIgnoreCase)
                .Replace(" OST", "", StringComparison.OrdinalIgnoreCase)
                .Replace(" original score", "", StringComparison.OrdinalIgnoreCase)
                .Replace(" television soundtrack", "", StringComparison.OrdinalIgnoreCase)
                .Trim();
            
            var preferred = albums
                .Select(a => new { Album = a, Score = ScoreAlbumMatch(a, baseQuery, null, null) })
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();
            
            if (preferred != null && preferred.Score >= 4)
                return preferred.Album;
                
            return null;
        }

        private static string GetBaseTitle(string name)
        {
            var idx = name.IndexOf(" (", StringComparison.Ordinal);
            if (idx > 0) return name.Substring(0, idx);
            idx = name.IndexOf(" [", StringComparison.Ordinal);
            if (idx > 0) return name.Substring(0, idx);
            return name;
        }

        private static string NormalizeTitle(string title)
        {
            // Remove caracteres especiais e normaliza espaços
            var normalized = Regex.Replace(title, @"[^a-zA-Z0-9\s]", "");
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();
            return normalized.ToLowerInvariant();
        }

        private static bool IsTitleMatchStrict(string albumName, string title)
        {
            var a = GetBaseTitle(albumName).Trim();
            var t = title.Trim();
            if (string.Equals(a, t, StringComparison.OrdinalIgnoreCase))
                return true;
            
            // Tenta comparação normalizada
            var aNorm = NormalizeTitle(a);
            var tNorm = NormalizeTitle(t);
            return string.Equals(aNorm, tNorm, StringComparison.OrdinalIgnoreCase);
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

            // Comparação exata do título
            if (IsTitleMatchStrict(name, title)) 
            {
                score += 20;
            }
            else
            {
                // Busca por título completo no nome do álbum
                var pattern = $"\\b{Regex.Escape(title)}\\b";
                if (Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase)) 
                {
                    score += 12;
                }
                else
                {
                    // Correspondência parcial de palavras
                    var titleWords = NormalizeTitle(title).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var nameNormalized = NormalizeTitle(name);
                    
                    // Filtra palavras muito curtas que podem gerar falsos positivos
                    var significantWords = titleWords.Where(w => w.Length >= 3).ToArray();
                    
                    if (significantWords.Length > 0)
                    {
                        var matchedWords = significantWords.Count(w => nameNormalized.Contains(w));
                        var matchRatio = (double)matchedWords / significantWords.Length;
                        
                        // Se tem palavras significativas, precisa de pelo menos 50% de match
                        if (matchRatio >= 0.9) score += 10;
                        else if (matchRatio >= 0.7) score += 7;
                        else if (matchRatio >= 0.5) score += 4;
                        else
                        {
                            // Match muito fraco - penaliza fortemente
                            score -= 5;
                        }
                    }
                    else if (titleWords.Length > 0)
                    {
                        // Se só tem palavras curtas, faz match simples
                        var matchedWords = titleWords.Count(w => nameNormalized.Contains(w));
                        var matchRatio = (double)matchedWords / titleWords.Length;
                        if (matchRatio >= 0.8) score += 6;
                        else score -= 3;
                    }
                }
            }

            var lower = name.ToLowerInvariant();
            if (lower.Contains("original motion picture soundtrack")) score += 5;
            if (lower.Contains("original series soundtrack")) score += 5;
            if (lower.Contains("original television soundtrack")) score += 5;
            if (lower.Contains("original score")) score += 4;
            if (lower.Contains("soundtrack")) score += 2;
            if (lower.Contains("ost")) score += 2;

            if (!string.IsNullOrWhiteSpace(composer) && (album.artists?.Any(a => string.Equals(a.name, composer, StringComparison.OrdinalIgnoreCase)) ?? false))
                score += 10;

            if (IsYearClose(album.release_date, year)) score += 5;

            return score;
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public class SpotifySearchResponse
        {
            public Albums albums { get; set; } = null!;
        }

        public class Albums
        {
            public List<SpotifyAlbum> items { get; set; } = null!;
        }

        public class SpotifyAlbum
        {
            public string id { get; set; } = null!;
            public string name { get; set; } = null!;
            public string release_date { get; set; } = null!;
            public ExternalUrls external_urls { get; set; } = null!;
            public List<SpotifyArtist> artists { get; set; } = null!;
        }

        public class SpotifyArtist
        {
            public string name { get; set; } = null!;
        }

        public class ExternalUrls
        {
            public string spotify { get; set; } = null!;
        }

        public class SpotifyTracksResponse
        {
            public List<SpotifyTrack> items { get; set; } = null!;
        }

        public class SpotifyTrack
        {
            public string id { get; set; } = null!;
            public string name { get; set; } = null!;
            public int duration_ms { get; set; }
            public List<SpotifyArtist> artists { get; set; } = null!;
            public ExternalUrls external_urls { get; set; } = null!;
            public string preview_url { get; set; } = null!;
        }
    }
}