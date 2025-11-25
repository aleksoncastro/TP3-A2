using MediaMatch.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace MediaMatch.Controllers.TADB
{
    /// <summary>
    /// Endpoints de consulta a artistas na API TheAudioDB.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ArtistsController : ControllerBase
    {
        private readonly AudioDbApiService _audioDb;

        public ArtistsController(AudioDbApiService audioDb)
        {
            _audioDb = audioDb;
        }

        /// <summary>
        /// Busca artistas por nome.
        /// </summary>
        /// <param name="name">Nome do artista.</param>
        [HttpGet("search")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
