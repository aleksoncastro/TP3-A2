using MediaMatch.DTO.Soundtrack;
using MediaMatch.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace MediaMatch.Controllers
{
    /// <summary>
    /// Endpoints para obter trilhas sonoras de filmes e séries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SoundtrackController : ControllerBase
    {
        private readonly SoundtrackAggregator _aggregator;

        public SoundtrackController(SoundtrackAggregator aggregator)
        {
            _aggregator = aggregator;
        }

        /// <summary>
        /// Obtém a trilha sonora provável para um filme.
        /// </summary>
        /// <param name="id">ID do filme no TMDB.</param>
        [HttpGet("movie/{id:int}")]
        [ProducesResponseType(typeof(SoundtrackDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMovie(int id)
        {
            var result = await _aggregator.GetMovieSoundtrackAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Obtém a trilha sonora provável para uma série.
        /// </summary>
        /// <param name="id">ID da série no TMDB.</param>
        [HttpGet("serie/{id:int}")]
        [ProducesResponseType(typeof(SoundtrackDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTv(int id)
        {
            var result = await _aggregator.GetSeriesSoundtrackAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
