using MediaMatch.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace MediaMatch.Controllers.TADB
{
    /// <summary>
    /// Endpoints de consulta a álbuns na API TheAudioDB.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AlbumsController : ControllerBase
    {
        private readonly AudioDbApiService _audioDb;

        public AlbumsController(AudioDbApiService audioDb)
        {
            _audioDb = audioDb;
        }

        /// <summary>
        /// Lista álbuns por ID de artista.
        /// </summary>
        /// <param name="artistId">ID do artista no TheAudioDB.</param>
        [HttpGet("{artistId:int}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAlbums(int artistId)
        {
            if (artistId <= 0)
                return BadRequest("ID inválido.");

            var result = await _audioDb.GetAlbumsAsync(artistId);

            // usa o nome exato do DTO: "album"
            if (result == null || result.album == null || !result.album.Any())
                return NotFound("Nenhum álbum encontrado.");

            return Ok(result.album);
        }
    }
}
