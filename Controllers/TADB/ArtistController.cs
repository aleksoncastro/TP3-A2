using MediaMatch.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediaMatch.Controllers.TADB
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtistsController : ControllerBase
    {
        private readonly AudioDbApiService _audioDb;

        public ArtistsController(AudioDbApiService audioDb)
        {
            _audioDb = audioDb;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchArtist([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Nome do artista é obrigatório.");

            var result = await _audioDb.SearchArtistAsync(name);

            // usa o nome exato do DTO: "artists"
            if (result == null || result.artists == null || !result.artists.Any())
                return NotFound("Artista não encontrado.");

            return Ok(result.artists);
        }
    }
}
