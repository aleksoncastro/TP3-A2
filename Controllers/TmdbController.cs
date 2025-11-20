using MediaMatch.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediaMatch.Controllers
{
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

            [HttpGet("popular")]
            public async Task<IActionResult> GetPopularMovies()
            {
                var result = await _service.GetPopularMoviesAsync();
                return Ok(result);
            }

            [HttpGet("search")]
            public async Task<IActionResult> Search([FromQuery] string q)
            {
                var result = await _service.SearchMoviesAsync(q);
                return Ok(result);
            }
        }
    }
}
