namespace MediaMatch.Controllers
{
    using global::MediaMatch.Models.TMDB;
    using global::MediaMatch.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Endpoints para listas de mídia de usuários e clubes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MediaListController : ControllerBase
    {
        private readonly MediaListService _service;

        public MediaListController(MediaListService service)
        {
            _service = service;
        }

        /// <summary>
        /// Cria uma nova lista de mídia.
        /// </summary>
        /// <param name="list">Dados da lista.</param>
        [HttpPost]
        [ProducesResponseType(typeof(MediaList), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(MediaList list)
        {
            var result = await _service.CreateListAsync(list);
            return Ok(result);
        }

        /// <summary>
        /// Lista as listas de mídia de um clube.
        /// </summary>
        /// <param name="clubId">ID do clube.</param>
        [HttpGet("club/{clubId}")]
        [ProducesResponseType(typeof(List<MediaList>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByClub(int clubId)
        {
            return Ok(await _service.GetListsByClubAsync(clubId));
        }

        /// <summary>
        /// Lista as listas de mídia de um usuário.
        /// </summary>
        /// <param name="userId">ID do usuário.</param>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(List<MediaList>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByUser(int userId)
        {
            return Ok(await _service.GetListsByUserAsync(userId));
        }
    }

}
