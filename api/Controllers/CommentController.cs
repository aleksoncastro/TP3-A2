using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediaMatch.Services;
using MediaMatch.DTO.Club;
using MediaMatch.Extensions;
using System.Security.Claims;

namespace MediaMatch.Controllers
{
    [ApiController]
    [Route("api/club/{clubId}/post/{postId}/comment")]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly CommentService _service;
        private readonly ILogger<CommentController> _logger;

        public CommentController(CommentService service, ILogger<CommentController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Cria um novo comentário em um post.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateComment(
            [FromRoute] int clubId,
            [FromRoute] int postId,
            [FromBody] CreateCommentDto dto)
        {
            var userId = GetCurrentUserId();
            var comment = await _service.CreateCommentAsync(postId, dto, userId);
            var commentDto = comment.ToDto(userId);
            return CreatedAtAction(nameof(GetComments), new { clubId, postId }, commentDto);
        }

        /// <summary>
        /// Lista todos os comentários de um post.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<CommentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetComments(
            [FromRoute] int clubId,
            [FromRoute] int postId)
        {
            var userId = GetCurrentUserId();
            var comments = await _service.GetPostCommentsAsync(postId, userId);
            return Ok(comments);
        }

        /// <summary>
        /// Atualiza um comentário.
        /// </summary>
        [HttpPut("{commentId}")]
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateComment(
            [FromRoute] int clubId,
            [FromRoute] int postId,
            [FromRoute] int commentId,
            [FromBody] UpdateCommentDto dto)
        {
            var userId = GetCurrentUserId();
            var comment = await _service.UpdateCommentAsync(commentId, dto, userId);
            var commentDto = comment.ToDto(userId);
            return Ok(commentDto);
        }

        /// <summary>
        /// Deleta um comentário.
        /// </summary>
        [HttpDelete("{commentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteComment(
            [FromRoute] int clubId,
            [FromRoute] int postId,
            [FromRoute] int commentId)
        {
            var userId = GetCurrentUserId();
            await _service.DeleteCommentAsync(commentId, userId);
            return NoContent();
        }
    }
}
