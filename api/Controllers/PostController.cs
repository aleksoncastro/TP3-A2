using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediaMatch.Services;
using MediaMatch.DTO.Club;
using MediaMatch.Extensions;
using System.Security.Claims;

namespace MediaMatch.Controllers
{
    [ApiController]
    [Route("api/club/{clubId}/post")]
    public class PostController : ControllerBase
    {
        private readonly PostService _service;
        private readonly ILogger<PostController> _logger;

        public PostController(PostService service, ILogger<PostController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int? TryGetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return null;
            return userId;
        }

        /// <summary>
        /// Cria um novo post no clube.
        /// </summary>
        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreatePost(
            [FromRoute] int clubId,
            [FromForm] CreatePostDto dto,
            List<IFormFile>? images)
        {
            var userId = TryGetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Token inválido ou ausente" });

            var post = await _service.CreatePostAsync(clubId, dto, userId.Value, images ?? new List<IFormFile>());
            var postDto = post.ToDto(userId.Value);
            return CreatedAtAction(nameof(GetPostById), new { clubId, postId = post.Id }, postDto);
        }

        /// <summary>
        /// Lista todos os posts de um clube.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<PostDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClubPosts(
            [FromRoute] int clubId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 20)
        {
            var userId = TryGetCurrentUserId() ?? 0;
            var posts = await _service.GetClubPostsAsync(clubId, userId, skip, take);
            return Ok(posts);
        }

        /// <summary>
        /// Obtém detalhes de um post específico.
        /// </summary>
        [HttpGet("{postId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PostDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPostById([FromRoute] int clubId, [FromRoute] int postId)
        {
            var userId = TryGetCurrentUserId() ?? 0;
            var post = await _service.GetByIdAsync(postId, userId);
            
            if (post == null)
                return NotFound(new { message = "Post não encontrado" });

            return Ok(post);
        }

        /// <summary>
        /// Atualiza um post.
        /// </summary>
        [HttpPut("{postId}")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(PostDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePost(
            [FromRoute] int clubId,
            [FromRoute] int postId,
            [FromForm] UpdatePostDto dto,
            List<IFormFile>? images)
        {
            var userId = TryGetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Token inválido ou ausente" });

            var post = await _service.UpdatePostAsync(postId, dto, userId.Value, images);
            var postDto = post.ToDto(userId.Value);
            return Ok(postDto);
        }

        /// <summary>
        /// Deleta um post.
        /// </summary>
        [HttpDelete("{postId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePost([FromRoute] int clubId, [FromRoute] int postId)
        {
            var userId = TryGetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Token inválido ou ausente" });

            await _service.DeletePostAsync(postId, userId.Value);
            return NoContent();
        }
    }
}
