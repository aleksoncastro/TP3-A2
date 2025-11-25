using MediaMatch.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace MediaMatch.Controllers.TADB
{
    /// <summary>
    /// Endpoints de consulta a faixas (músicas) na API TheAudioDB.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TracksController : ControllerBase
    {
        private readonly AudioDbApiService _audioDb;

        public TracksController(AudioDbApiService audioDb)
        {
            _audioDb = audioDb;
        }

        /// <summary>
        /// Lista faixas por ID de álbum.
        /// </summary>
        /// <param name="albumId">ID do álbum no TheAudioDB.</param>
        [HttpGet("{albumId:int}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTracks(int albumId)
        {
            if (albumId <= 0)
                return BadRequest("ID inválido.");

            var result = await _audioDb.GetTracksAsync(albumId);

            // usa o nome exato do DTO: "track"
            if (result == null || result.track == null || !result.track.Any())
                return NotFound("Nenhuma música encontrada.");

            return Ok(result.track);
        }
    }
}
