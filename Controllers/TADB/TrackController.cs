using MediaMatch.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediaMatch.Controllers.TADB
{
    [ApiController]
    [Route("api/[controller]")]
    public class TracksController : ControllerBase
    {
        private readonly AudioDbApiService _audioDb;

        public TracksController(AudioDbApiService audioDb)
        {
            _audioDb = audioDb;
        }

        [HttpGet("{albumId:int}")]
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
