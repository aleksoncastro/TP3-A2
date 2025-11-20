using MediaMatch.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediaMatch.Controllers.TADB
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlbumsController : ControllerBase
    {
        private readonly AudioDbApiService _audioDb;

        public AlbumsController(AudioDbApiService audioDb)
        {
            _audioDb = audioDb;
        }

        [HttpGet("{artistId:int}")]
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
