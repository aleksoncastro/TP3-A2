using MediaMatch.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
 

namespace MediaMatch.Controllers
{
    /// <summary>
    /// Endpoints para consulta de filmes e séries via TMDB.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class TmdbController : ControllerBase
    {
        private readonly TmdbService _service;
        
        
        public TmdbController(TmdbService service)
        {
            _service = service;
        }

            /// <summary>
            /// Lista filmes populares.
            /// </summary>
            /// <param name="language">Idioma dos resultados (padrão: pt-BR).</param>
            /// <param name="page">Número da página (padrão: 1).</param>
            /// <param name="region">Região ISO-3166 para filtragem opcional.</param>
            [HttpGet("movies/popular")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetPopularMovies([FromQuery] string language = "pt-BR", [FromQuery] int page = 1, [FromQuery] string? region = null)
            {
                var result = await _service.GetPopularMoviesAsync(language, page, region);
                return Content(result, "application/json");
            }
            /// <summary>
            /// Lista filmes em cartaz (now playing).
            /// </summary>
            [HttpGet("movies/now_playing")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetNowPlayingMovies()
            {
                var result = await _service.GetNowPlayingMoviesAsync();
                return Content(result, "application/json");
            }
            /// <summary>
            /// Lista filmes com melhores avaliações.
            /// </summary>
            /// <param name="language">Idioma dos resultados (padrão: pt-BR).</param>
            /// <param name="page">Número da página (padrão: 1).</param>
            /// <param name="region">Região ISO-3166 para filtragem opcional.</param>
            [HttpGet("movies/top_rated")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetTopRatedMovies([FromQuery] string language = "pt-BR", [FromQuery] int page = 1, [FromQuery] string? region = null)
            {
                var result = await _service.GetTopRatedMoviesAsync(language, page, region);
                return Content(result, "application/json");
            }
            /// <summary>
            /// Lista filmes que serão lançados (upcoming).
            /// </summary>
            /// <param name="language">Idioma dos resultados (padrão: pt-BR).</param>
            /// <param name="page">Número da página (padrão: 1).</param>
            /// <param name="region">Região ISO-3166 para filtragem opcional.</param>
            [HttpGet("movies/upcoming")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetUpcomingMovies([FromQuery] string language = "pt-BR", [FromQuery] int page = 1, [FromQuery] string? region = null)
            {
                var result = await _service.GetUpcomingMoviesAsync(language, page, region);
                return Content(result, "application/json");
            }
            /// <summary>
            /// Lista filmes em tendência (trending).
            /// </summary>
            /// <param name="language">Idioma dos resultados (padrão: pt-BR).</param>
            /// <param name="page">Número da página (padrão: 1).</param>
            /// <param name="region">Região ISO-3166 para filtragem opcional.</param>
            [HttpGet("movies/trending")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetTrendingMovies([FromQuery] string language = "pt-BR", [FromQuery] int page = 1, [FromQuery] string? region = null, [FromQuery] string timeWindow = "day")
            {
                var result = await _service.GetTrendingMoviesAsync(language, page, region, timeWindow);
                return Content(result, "application/json");
            }
            /// <summary>
            /// Detalhes de um filme por ID.
            /// </summary>
            /// <param name="id">ID do filme no TMDB.</param>
            [HttpGet("movies/details")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetMovieDetails([FromQuery] int id)
            {
                var result = await _service.GetMovieDetailsAsync(id);
                return Content(result, "application/json");
            }
            [HttpGet("movies/credits")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetMovieCredits([FromQuery] int id)
            {
                var result = await _service.GetMovieCreditsAsync(id);
                return Content(result, "application/json");
            }
            /// <summary>
            /// Busca de filmes.
            /// </summary>
            /// <param name="q">Texto da consulta.</param>
            [HttpGet("movies/search")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] bool include_adult = false, [FromQuery] string language = "pt-BR", [FromQuery] int? primary_release_year = null, [FromQuery] int page = 1, [FromQuery] string? region = null, [FromQuery] string? year = null)
            {
                var result = await _service.SearchMoviesAsync(q, include_adult, language, primary_release_year, page, region, year);
                return Content(result, "application/json");
            }
            /// <summary>
            /// Busca combinada (filmes, séries, pessoas).
            /// </summary>
            /// <param name="q">Texto da consulta.</param>
            /// <param name="include_adult">Inclui conteúdo adulto (padrão: false).</param>
            /// <param name="page">Número da página (padrão: 1).</param>
            [HttpGet("multi/search")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> SearchMulti([FromQuery] string q, [FromQuery] bool include_adult = false, [FromQuery] int page = 1)
            {
                var result = await _service.SearchMultiAsync(q, include_adult, page);
                return Content(result, "application/json");
            }

            /// <summary>
            /// Lista séries populares.
            /// </summary>
            /// <param name="language">Idioma dos resultados (padrão: pt-BR).</param>
            /// <param name="page">Número da página (padrão: 1).</param>
            /// <param name="region">Região ISO-3166 para filtragem opcional.</param>
            [HttpGet("series/popular")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetPopularSeries([FromQuery] string language = "pt-BR", [FromQuery] int page = 1, [FromQuery] string? region = null)
            {
                var result = await _service.GetPopularSeriesAsync(language, page, region);
                return Content(result, "application/json");
            }
            /// <summary>
            /// Lista séries com episódios indo ao ar hoje.
            /// </summary>
            /// <param name="language">Idioma dos resultados (padrão: pt-BR).</param>
            /// <param name="page">Número da página (padrão: 1).</param>
            /// <param name="region">Região ISO-3166 para filtragem opcional.</param>
            [HttpGet("series/airing_today")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetAiringTodaySeries([FromQuery] string language = "pt-BR", [FromQuery] int page = 1, [FromQuery] string? region = null)
            {
                var result = await _service.GetAiringTodaySeriesAsync(language, page, region);
                return Content(result, "application/json");
            }
            /// <summary>
            /// Lista séries atualmente no ar.
            /// </summary>
            /// <param name="language">Idioma dos resultados (padrão: pt-BR).</param>
            /// <param name="page">Número da página (padrão: 1).</param>
            /// <param name="region">Região ISO-3166 para filtragem opcional.</param>
            [HttpGet("series/on_the_air")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetOnTheAirSeries([FromQuery] string language = "pt-BR", [FromQuery] int page = 1, [FromQuery] string? region = null)
            {
                var result = await _service.GetOnTheAirSeriesAsync(language, page, region);
                return Content(result, "application/json");
            }
            /// <summary>
            /// Lista séries com melhores avaliações.
            /// </summary>
            /// <param name="language">Idioma dos resultados (padrão: pt-BR).</param>
            /// <param name="page">Número da página (padrão: 1).</param>
            /// <param name="region">Região ISO-3166 para filtragem opcional.</param>
            [HttpGet("series/top_rated")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetTopRatedSeries([FromQuery] string language = "pt-BR", [FromQuery] int page = 1, [FromQuery] string? region = null)
            {
                var result = await _service.GetTopRatedSeriesAsync(language, page, region);
                return Content(result, "application/json");
            }
            /// <summary>
            /// Busca de séries.
            /// </summary>
            /// <param name="q">Texto da consulta.</param>
            [HttpGet("series/search")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> SearchSeries([FromQuery] string q, [FromQuery] bool include_adult = false, [FromQuery] string language = "pt-BR", [FromQuery] int? first_air_year = null, [FromQuery] int page = 1, [FromQuery] string? region = null)
            {
                var result = await _service.SearchSeriesAsync(q, include_adult, language, first_air_year, page, region);
                return Content(result, "application/json");
            }
            /// <summary>
            /// Detalhes de uma série por ID.
            /// </summary>
            /// <param name="id">ID da série no TMDB.</param>
            [HttpGet("series/details")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetSeriesDetails([FromQuery] int id)
            {
                var result = await _service.GetSeriesDetailsAsync(id);
                return Content(result, "application/json");
            }
            [HttpGet("series/credits")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetSeriesCredits([FromQuery] int id)
            {
                var result = await _service.GetSeriesCreditsAsync(id);
                return Content(result, "application/json");
            }

            /// <summary>
            /// Detalhes de uma coleção por ID.
            /// </summary>
            /// <param name="id">ID da coleção no TMDB.</param>
            [HttpGet("collection/details")]
            [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
            public async Task<IActionResult> GetCollectionDetails([FromQuery] int id)
            {
                var result = await _service.GetCollectionAsync(id);
                return Content(result, "application/json");
            }
    }
}
