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

        // ======================================================
        // 1. ARTISTA (search.php?s=name)
        // ======================================================
        [HttpGet("search-artist")]
        public async Task<IActionResult> SearchArtist([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Digite o nome do artista.");

            var result = await _apiService.SearchArtistAsync(name);

            if (result == null || result.artists == null || !result.artists.Any())
            {
                return NotFound($"Artista '{name}' não encontrado no TheAudioDB.");
            }

            return Ok(result.artists);
        }

        // ======================================================
        // 2. TODOS OS ÁLBUNS - POR ID (album.php?i=id)
        // ======================================================
        [HttpGet("albums-by-id")]
        public async Task<ActionResult<List<AudioDbAlbumDto>>> GetAlbumsByArtistId([FromQuery] int artistId)
        {
            var response = await _apiService.GetAlbumsAsync(artistId);

            if (response == null || response.album == null || !response.album.Any())
                return NotFound("Nenhum álbum encontrado para este artista.");

            return Ok(response.album);
        }

        // ======================================================
        // NOVO: TODOS OS ÁLBUNS - POR NOME DO ARTISTA (searchalbum.php?s=name)
        // ======================================================
        [HttpGet("albums-by-name")]
        public async Task<ActionResult<List<AudioDbAlbumDto>>> GetAlbumsByArtistName([FromQuery] string artistName)
        {
            if (string.IsNullOrWhiteSpace(artistName)) 
                return BadRequest("Nome do artista obrigatório.");

            var response = await _apiService.GetAlbumsByArtistNameAsync(artistName);

            if (response == null || response.album == null || !response.album.Any())
                return NotFound("Nenhum álbum encontrado para este artista.");

            return Ok(response.album);
        }

        // ======================================================
        // 3. ÁLBUM ÚNICO (searchalbum.php?s=artist&a=album)
        // ======================================================
        [HttpGet("album")]
        public async Task<ActionResult<List<AudioDbAlbumDto>>> SearchAlbum([FromQuery] string artist, [FromQuery] string album)
        {
            if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(album))
                return BadRequest("O nome do artista e do álbum são obrigatórios.");

            var response = await _apiService.SearchAlbumByNameAsync(artist, album);

            if (response == null || response.album == null || !response.album.Any())
                return NotFound("Álbum não encontrado.");

            return Ok(response.album);
        }

        // ======================================================
        // 4. FAIXAS / MÚSICA (searchtrack.php)
        // ======================================================
        [HttpGet("track")]
        public async Task<ActionResult<List<AudioDbTrackDto>>> SearchTrack([FromQuery] string query, [FromQuery] string? artist = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("O nome da música é obrigatório.");

            AudioDbTrackResponse? response;

            if (!string.IsNullOrWhiteSpace(artist))
            {
                response = await _apiService.SearchTrackAsync(artist, query);
            }
            else
            {
                response = await _apiService.SearchTrackByNameAsync(query);
            }

            if (response == null || response.track == null || !response.track.Any())
                return NotFound("Nenhuma música encontrada.");

            return Ok(response.track);
        }

        [HttpGet("tracks-by-album")]
        public async Task<ActionResult<List<AudioDbTrackDto>>> GetTracksByAlbumId([FromQuery] int albumId)
        {
            var response = await _apiService.GetTracksAsync(albumId);

            if (response == null || response.track == null || !response.track.Any())
                return NotFound("Nenhuma faixa encontrada para este álbum.");

            return Ok(response.track);
        }

        // ======================================================
        // NOVO: DISCOGRAFIA POR NOME (discography.php?s=name)
        // ======================================================
        [HttpGet("discography")]
        public async Task<ActionResult<List<AudioDbAlbumDto>>> GetDiscography([FromQuery] string artistName)
        {
            if (string.IsNullOrWhiteSpace(artistName)) 
                return BadRequest("Nome do artista obrigatório.");

            var response = await _apiService.GetDiscographyByNameAsync(artistName);

            if (response == null || response.album == null || !response.album.Any())
                return NotFound("Discografia não encontrada.");

            // A discografia retorna apenas o Ano e o Nome do álbum geralmente
            return Ok(response.album);
        }

        // ======================================================
        // NOVO: DISCOGRAFIA POR MBID (discography-mb.php?s=mbid)
        // ======================================================
        [HttpGet("discography-mbid")]
        public async Task<ActionResult<List<AudioDbAlbumDto>>> GetDiscographyByMbId([FromQuery] string mbId)
        {
            if (string.IsNullOrWhiteSpace(mbId)) 
                return BadRequest("MusicBrainz ID obrigatório.");

            var response = await _apiService.GetDiscographyByMbIdAsync(mbId);

            if (response == null || response.album == null || !response.album.Any())
                return NotFound("Discografia não encontrada.");

            return Ok(response.album);
        }

        
        /// <summary>
        /// Retorna as músicas mais amadas.
        /// URL: GET api/music/search/popular-tracks
        /// </summary>
        [HttpGet("popular-tracks")]
        public async Task<IActionResult> GetPopularTracks([FromQuery] int page = 1)
        {
            // 1. Busca todas as 50 músicas do Service
            var allTracks = await _apiService.GetMostLovedTracksAsync();

            if (allTracks == null || !allTracks.Any())
                return Ok(new { results = new List<AudioDbTrackDto>() });

            // 2. Paginação em Memória (Simulação)
            // O AudioDB retorna tudo de uma vez, então cortamos a lista aqui
            int pageSize = 20;
            var pagedTracks = allTracks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 3. Retorna no formato { results: [...] } que o Angular espera
            return Ok(new 
            { 
                page = page,
                results = pagedTracks,
                total_pages = (int)Math.Ceiling(allTracks.Count / (double)pageSize),
                total_results = allTracks.Count
            });
        }

        /// <summary>
        /// Retorna os álbuns mais amados.
        /// URL: GET api/music/search/popular-albums
        /// </summary>
        [HttpGet("popular-albums")]
        public async Task<IActionResult> GetPopularAlbums([FromQuery] int page = 1)
        {
            var allAlbums = await _apiService.GetMostLovedAlbumsAsync();

            if (allAlbums == null || !allAlbums.Any())
                return Ok(new { results = new List<AudioDbAlbumDto>() });

            int pageSize = 20;
            var pagedAlbums = allAlbums
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new 
            { 
                page = page,
                results = pagedAlbums,
                total_pages = (int)Math.Ceiling(allAlbums.Count / (double)pageSize),
                total_results = allAlbums.Count
            });
        }
    }
}