namespace MediaMatch.Controllers
{
    using global::MediaMatch.Models.TMDB;
    using global::MediaMatch.Services;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class ClubController : ControllerBase
    {
        private readonly ClubService _service;

        public ClubController(ClubService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClub(Club club)
        {
            var result = await _service.CreateClubAsync(club);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var club = await _service.GetByIdAsync(id);
            if (club == null) return NotFound();
            return Ok(club);
        }

        [HttpPost("{clubId}/members/{userId}")]
        public async Task<IActionResult> AddMember(int clubId, int userId)
        {
            var result = await _service.AddMemberAsync(clubId, userId);

            if (!result)
                return BadRequest("Usuário já é membro.");

            return Ok("Membro adicionado com sucesso!");
        }
    }

}

