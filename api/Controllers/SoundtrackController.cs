using MediaMatch.DTO.Soundtrack;
using MediaMatch.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediaMatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SoundtrackController : ControllerBase
    {
        private readonly SoundtrackAggregator _aggregator;

        public SoundtrackController(SoundtrackAggregator aggregator)
        {
            _aggregator = aggregator;
        }

        [HttpGet("movie/{id:int}")]
        public async Task<IActionResult> GetMovie(int id)
        {
            var result = await _aggregator.GetMovieSoundtrackAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("tv/{id:int}")]
        public async Task<IActionResult> GetTv(int id)
        {
            var result = await _aggregator.GetSeriesSoundtrackAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}