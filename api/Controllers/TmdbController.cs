using MediaMatch.Services;
using Microsoft.AspNetCore.Mvc;
 

namespace MediaMatch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TmdbController : ControllerBase
    {
        private readonly TmdbService _service;
        
        
        public TmdbController(TmdbService service)
        {
            _service = service;
        }

            [HttpGet("movies/popular")]
            public async Task<IActionResult> GetPopularMovies()
            {
                var result = await _service.GetPopularMoviesAsync();
                return Ok(result);
            }
            [HttpGet("movies/details")]
            public async Task<IActionResult> GetMovieDetails([FromQuery] int id)
            {
                var result = await _service.GetMovieDetailsAsync(id);
                return Ok(result);
            }
            [HttpGet("movies/search")]
            public async Task<IActionResult> Search([FromQuery] string q)
            {
                var result = await _service.SearchMoviesAsync(q);
                return Ok(result);
            }

            [HttpGet("series/popular")]
            public async Task<IActionResult> GetPopularSeries()
            {
                var result = await _service.GetPopularSeriesAsync();
                return Ok(result);
            }
            [HttpGet("series/search")]
            public async Task<IActionResult> SearchSeries([FromQuery] string q)
            {
                var result = await _service.SearchSeriesAsync(q);
                return Ok(result);
            }
            [HttpGet("series/details")]
            public async Task<IActionResult> GetSeriesDetails([FromQuery] int id)
            {
                var result = await _service.GetSeriesDetailsAsync(id);
                return Ok(result);
            }
        
    }
}
