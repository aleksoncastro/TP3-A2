namespace MediaMatch.Controllers
{
    using global::MediaMatch.Models.TMDB;
    using global::MediaMatch.Services;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class MediaListController : ControllerBase
    {
        private readonly MediaListService _service;

        public MediaListController(MediaListService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(MediaList list)
        {
            var result = await _service.CreateListAsync(list);
            return Ok(result);
        }

        [HttpGet("club/{clubId}")]
        public async Task<IActionResult> GetByClub(int clubId)
        {
            return Ok(await _service.GetListsByClubAsync(clubId));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            return Ok(await _service.GetListsByUserAsync(userId));
        }
    }

}
