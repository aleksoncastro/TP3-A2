namespace MediaMatch.Services
{
    public class TmdbService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public TmdbService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["TMDB:ApiKey"]!;
            _baseUrl = config["TMDB:BaseUrl"]!;
        }

        public async Task<string> GetCollectionAsync(int collectionId, string language = "pt-BR")
        {
            var url = $"{_baseUrl}collection/{collectionId}?api_key={_apiKey}&language={language}";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetPopularMoviesAsync(string language = "pt-BR", int page = 1, string? region = null)
        {
            var url = $"{_baseUrl}movie/popular?api_key={_apiKey}&language={language}&page={page}" + (string.IsNullOrWhiteSpace(region) ? string.Empty : $"&region={region}");

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetNowPlayingMoviesAsync()
        {
            var url = $"{_baseUrl}movie/now_playing?api_key={_apiKey}&language=pt-BR";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetTopRatedMoviesAsync(string language = "pt-BR", int page = 1, string? region = null)
        {
            var url = $"{_baseUrl}movie/top_rated?api_key={_apiKey}&language={language}&page={page}" + (string.IsNullOrWhiteSpace(region) ? string.Empty : $"&region={region}");

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetUpcomingMoviesAsync(string language = "pt-BR", int page = 1, string? region = null)
        {
            var url = $"{_baseUrl}movie/upcoming?api_key={_apiKey}&language={language}&page={page}" + (string.IsNullOrWhiteSpace(region) ? string.Empty : $"&region={region}");

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetTrendingMoviesAsync(string language = "pt-BR", int page = 1, string? region = null, string timeWindow = "day")
        {
            var url = $"{_baseUrl}trending/movie/{timeWindow}?api_key={_apiKey}&language={language}&page={page}" + (string.IsNullOrWhiteSpace(region) ? string.Empty : $"&region={region}");

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetMovieDetailsAsync(int movieId)
        {
            var url = $"{_baseUrl}movie/{movieId}?api_key={_apiKey}&language=pt-BR";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetMovieCreditsAsync(int movieId)
        {
            var url = $"{_baseUrl}movie/{movieId}/credits?api_key={_apiKey}&language=pt-BR";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> SearchMoviesAsync(string query, bool includeAdult = false, string language = "pt-BR", int? primaryReleaseYear = null, int page = 1, string? region = null, string? year = null)
        {
            var url = $"{_baseUrl}search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(query)}&language={language}&include_adult={includeAdult.ToString().ToLower()}&page={page}";

            if (primaryReleaseYear.HasValue)
                url += $"&primary_release_year={primaryReleaseYear.Value}";
            if (!string.IsNullOrEmpty(region))
                url += $"&region={Uri.EscapeDataString(region)}";
            if (!string.IsNullOrEmpty(year))
                url += $"&year={Uri.EscapeDataString(year)}";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> SearchMultiAsync(string query, bool includeAdult = false, int page = 1)
        {
            var url = $"{_baseUrl}search/multi?api_key={_apiKey}&query={query}&include_adult={includeAdult.ToString().ToLower()}&language=pt-BR&page={page}";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetPopularSeriesAsync(string language = "pt-BR", int page = 1, string? region = null)
        {
            var url = $"{_baseUrl}tv/popular?api_key={_apiKey}&language={language}&page={page}" + (string.IsNullOrWhiteSpace(region) ? string.Empty : $"&region={region}");

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetAiringTodaySeriesAsync(string language = "pt-BR", int page = 1, string? region = null)
        {
            var url = $"{_baseUrl}tv/airing_today?api_key={_apiKey}&language={language}&page={page}" + (string.IsNullOrWhiteSpace(region) ? string.Empty : $"&region={region}");

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetOnTheAirSeriesAsync(string language = "pt-BR", int page = 1, string? region = null)
        {
            var url = $"{_baseUrl}tv/on_the_air?api_key={_apiKey}&language={language}&page={page}" + (string.IsNullOrWhiteSpace(region) ? string.Empty : $"&region={region}");

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> GetTopRatedSeriesAsync(string language = "pt-BR", int page = 1, string? region = null)
        {
            var url = $"{_baseUrl}tv/top_rated?api_key={_apiKey}&language={language}&page={page}" + (string.IsNullOrWhiteSpace(region) ? string.Empty : $"&region={region}");

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetSeriesDetailsAsync(int seriesId)
        {
            var url = $"{_baseUrl}tv/{seriesId}?api_key={_apiKey}&language=pt-BR";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetSeriesCreditsAsync(int seriesId)
        {
            var url = $"{_baseUrl}tv/{seriesId}/credits?api_key={_apiKey}&language=pt-BR";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        public async Task<string> SearchSeriesAsync(string query, bool includeAdult = false, string language = "pt-BR", int? firstAirYear = null, int page = 1, string? region = null)
        {
            var url = $"{_baseUrl}search/tv?api_key={_apiKey}&query={Uri.EscapeDataString(query)}&language={language}&include_adult={includeAdult.ToString().ToLower()}&page={page}";

            if (firstAirYear.HasValue)
                url += $"&first_air_year={firstAirYear.Value}";
            if (!string.IsNullOrEmpty(region))
                url += $"&region={Uri.EscapeDataString(region)}";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}