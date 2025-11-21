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

        public async Task<string> GetPopularMoviesAsync()
        {
            var url = $"{_baseUrl}movie/popular?api_key={_apiKey}&language=pt-BR";

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
        public async Task<string> SearchMoviesAsync(string query)
        {
            var url = $"{_baseUrl}search/movie?api_key={_apiKey}&query={query}&language=pt-BR";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetPopularSeriesAsync()
        {
            var url = $"{_baseUrl}tv/popular?api_key={_apiKey}&language=pt-BR";

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
        public async Task<string> SearchSeriesAsync(string query)
        {
            var url = $"{_baseUrl}search/tv?api_key={_apiKey}&query={query}&language=pt-BR";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}