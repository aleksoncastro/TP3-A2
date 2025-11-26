using MediaMatch.DTO.TADB;
using MediaMatch.Services;
using Microsoft.AspNetCore.Mvc;

namespace MediaMatch.Controllers
{
    [ApiController]
    [Route("api/music/search")]
    public class AudioDbSearchController : ControllerBase
    {
        private readonly AudioDbApiService _apiService;

        public AudioDbSearchController(AudioDbApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Busca artistas pelo nome. Ex: GET api/music/search/artist?name=Coldplay
        /// </summary>
        // GET: api/music/search-artist?name=Coldplay
        [HttpGet("search-artist")]
        public async Task<IActionResult> SearchArtist([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Digite o nome do artista.");

            var result = await _apiService.SearchArtistAsync(name);

            // Verifica se o resultado é nulo ou se a lista de artistas está vazia
            if (result == null || result.artists == null || !result.artists.Any())
            {
                return NotFound($"Artista '{name}' não encontrado no TheAudioDB.");
            }

            return Ok(result.artists);
        }

        /// <summary>
        /// Busca álbuns de um artista específico. Ex: GET api/music/search/albums-by-id?artistId=111239
        /// </summary>
        [HttpGet("albums-by-id")]
        public async Task<ActionResult<List<AudioDbAlbumDto>>> GetAlbumsByArtistId([FromQuery] int artistId)
        {
            var response = await _apiService.GetAlbumsAsync(artistId);

            if (response == null || response.album == null || !response.album.Any())
                return NotFound("Nenhum álbum encontrado para este artista.");

            return Ok(response.album);
        }

        /// <summary>
        /// Busca um álbum específico pelo nome do artista e do álbum. 
        /// Ex: GET api/music/search/album?artist=Nirvana&album=Nevermind
        /// </summary>
        [HttpGet("album")]
        public async Task<ActionResult<List<AudioDbAlbumDto>>> SearchAlbum([FromQuery] string artist, [FromQuery] string album)
        {
            if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(album))
                return BadRequest("O nome do artista e do álbum são obrigatórios para esta busca.");

            var response = await _apiService.SearchAlbumByNameAsync(artist, album);

            if (response == null || response.album == null || !response.album.Any())
                return NotFound("Álbum não encontrado.");

            return Ok(response.album);
        }

        /// <summary>
        /// Busca faixas pelo nome. Pode filtrar por artista opcionalmente.
        /// Ex: GET api/music/search/track?query=Hello
        /// Ex: GET api/music/search/track?query=Hello&artist=Adele
        /// </summary>
        [HttpGet("track")]
        public async Task<ActionResult<List<AudioDbTrackDto>>> SearchTrack([FromQuery] string query, [FromQuery] string? artist = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("O nome da música é obrigatório.");

            AudioDbTrackResponse? response;

            if (!string.IsNullOrWhiteSpace(artist))
            {
                // Busca mais precisa: Música + Artista
                response = await _apiService.SearchTrackAsync(artist, query);
            }
            else
            {
                // Busca genérica: Apenas nome da Música
                response = await _apiService.SearchTrackByNameAsync(query);
            }

            if (response == null || response.track == null || !response.track.Any())
                return NotFound("Nenhuma música encontrada.");

            return Ok(response.track);
        }

        /// <summary>
        /// Busca todas as faixas de um álbum pelo ID. Ex: GET api/music/search/tracks-by-album?albumId=2115888
        /// </summary>
        [HttpGet("tracks-by-album")]
        public async Task<ActionResult<List<AudioDbTrackDto>>> GetTracksByAlbumId([FromQuery] int albumId)
        {
            var response = await _apiService.GetTracksAsync(albumId);

            if (response == null || response.track == null || !response.track.Any())
                return NotFound("Nenhuma faixa encontrada para este álbum.");

            return Ok(response.track);
        }
    }
}