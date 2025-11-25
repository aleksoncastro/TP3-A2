namespace MediaMatch.Controllers
{
    using global::MediaMatch.Models.TMDB;
    using global::MediaMatch.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// Endpoints para gerenciar clubes de mídia.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClubController : ControllerBase
    {
        private readonly ClubService _service;

        public ClubController(ClubService service)
        {
            _service = service;
        }

        /// <summary>
        /// Cria um novo clube.
        /// </summary>
        /// <param name="club">Dados do clube a ser criado.</param>
        [HttpPost]
        [ProducesResponseType(typeof(Club), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateClub(Club club)
        {
            var result = await _service.CreateClubAsync(club);
            return Ok(result);
        }

        /// <summary>
        /// Lista todos os clubes.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<Club>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        /// <summary>
        /// Obtém um clube por ID.
        /// </summary>
        /// <param name="id">ID do clube.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Club), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var club = await _service.GetByIdAsync(id);
            if (club == null) return NotFound();
            return Ok(club);
        }

        /// <summary>
        /// Adiciona um membro ao clube.
        /// </summary>
        /// <param name="clubId">ID do clube.</param>
        /// <param name="userId">ID do usuário.</param>
        [HttpPost("{clubId}/members/{userId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddMember(int clubId, int userId)
        {
            var result = await _service.AddMemberAsync(clubId, userId);

            if (!result)
                return BadRequest("Usuário já é membro.");

            return Ok("Membro adicionado com sucesso!");
        }
    }

}

