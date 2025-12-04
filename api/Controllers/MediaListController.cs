using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediaMatch.Services;
using MediaMatch.DTO.Club;
using System.Security.Claims;

namespace MediaMatch.Controllers
{
    [ApiController]
    [Route("api/club/{clubId}/list")]
    [Authorize]
    public class MediaListController : ControllerBase
    {
        private readonly MediaListService _service;
        private readonly ILogger<MediaListController> _logger;

        public MediaListController(MediaListService service, ILogger<MediaListController> logger)
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
        /// Cria uma nova lista no clube (apenas admins)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(MediaListDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateList([FromRoute] int clubId, [FromBody] CreateMediaListDto dto)
        {
            var userId = GetCurrentUserId();
            var list = await _service.CreateListAsync(clubId, dto, userId);
            
            var listDto = new MediaListDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                IsPublic = list.IsPublic,
                CreatedAt = list.CreatedAt,
                ClubId = list.ClubId
            };

            return CreatedAtAction(nameof(GetListDetail), new { clubId, listId = list.Id }, listDto);
        }

        /// <summary>
        /// Lista todas as listas do clube
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<MediaListDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClubLists([FromRoute] int clubId)
        {
            var userId = GetCurrentUserId();
            var lists = await _service.GetClubListsAsync(clubId, userId);
            return Ok(lists);
        }

        /// <summary>
        /// Obtém detalhes de uma lista específica
        /// </summary>
        [HttpGet("{listId}")]
        [ProducesResponseType(typeof(MediaListDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetListDetail([FromRoute] int clubId, [FromRoute] int listId)
        {
            var userId = GetCurrentUserId();
            var list = await _service.GetListDetailAsync(listId, userId);
            
            if (list == null)
                return NotFound(new { message = "Lista não encontrada" });

            return Ok(list);
        }

        /// <summary>
        /// Atualiza uma lista (apenas admins)
        /// </summary>
        [HttpPut("{listId}")]
        [ProducesResponseType(typeof(MediaListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateList(
            [FromRoute] int clubId,
            [FromRoute] int listId,
            [FromBody] UpdateMediaListDto dto)
        {
            var userId = GetCurrentUserId();
            var list = await _service.UpdateListAsync(listId, dto, userId);
            
            var listDto = new MediaListDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                IsPublic = list.IsPublic,
                CreatedAt = list.CreatedAt,
                ClubId = list.ClubId
            };

            return Ok(listDto);
        }

        /// <summary>
        /// Deleta uma lista (apenas admins)
        /// </summary>
        [HttpDelete("{listId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteList([FromRoute] int clubId, [FromRoute] int listId)
        {
            var userId = GetCurrentUserId();
            await _service.DeleteListAsync(listId, userId);
            return NoContent();
        }

        // ===== ITENS DA LISTA =====

        /// <summary>
        /// Adiciona um filme/série à lista (apenas admins)
        /// </summary>
        [HttpPost("{listId}/item")]
        [ProducesResponseType(typeof(MediaListItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddItemToList(
            [FromRoute] int clubId,
            [FromRoute] int listId,
            [FromBody] AddMediaListItemDto dto)
        {
            var userId = GetCurrentUserId();
            var item = await _service.AddItemToListAsync(listId, dto, userId);
            
            var itemDto = new MediaListItemDto
            {
                Id = item.Id,
                MediaListId = item.MediaListId,
                MediaItemId = item.MediaItemId,
                AddedAt = item.AddedAt,
                Note = item.Note,
                TmdbId = item.MediaItem.TmdbId,
                Title = item.MediaItem.Title,
                PosterPath = item.MediaItem.PosterUrl,
                MediaType = item.MediaItem.Type == Models.TMDB.MediaType.Movie ? "movie" : "tv",
                Rating = item.MediaItem.Rating,
                ReleaseDate = item.MediaItem.ReleaseDate?.ToString("yyyy-MM-dd")
            };

            return CreatedAtAction(nameof(GetListDetail), new { clubId, listId }, itemDto);
        }

        /// <summary>
        /// Remove um item da lista (apenas admins)
        /// </summary>
        [HttpDelete("{listId}/item/{itemId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveItemFromList(
            [FromRoute] int clubId,
            [FromRoute] int listId,
            [FromRoute] int itemId)
        {
            var userId = GetCurrentUserId();
            await _service.RemoveItemFromListAsync(listId, itemId, userId);
            return NoContent();
        }

        // ===== COMENTÁRIOS =====

        /// <summary>
        /// Adiciona um comentário ou sugestão à lista
        /// </summary>
        [HttpPost("{listId}/comment")]
        [ProducesResponseType(typeof(MediaListCommentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateComment(
            [FromRoute] int clubId,
            [FromRoute] int listId,
            [FromBody] CreateMediaListCommentDto dto)
        {
            var userId = GetCurrentUserId();
            var comment = await _service.CreateCommentAsync(listId, dto, userId);
            
            var commentDto = new MediaListCommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                Type = comment.Type,
                SuggestedMediaId = comment.SuggestedMediaId,
                SuggestedMediaType = comment.SuggestedMediaType,
                SuggestedMediaTitle = comment.SuggestedMediaTitle,
                SuggestedMediaPosterPath = comment.SuggestedMediaPosterPath,
                AuthorId = comment.AuthorId,
                AuthorName = comment.Author.UserName,
                AuthorProfilePictureUrl = comment.Author.ProfilePictureUrl,
                CanEdit = comment.AuthorId == userId,
                CanDelete = comment.AuthorId == userId
            };

            return CreatedAtAction(nameof(GetListDetail), new { clubId, listId }, commentDto);
        }

        /// <summary>
        /// Deleta um comentário (autor ou admin)
        /// </summary>
        [HttpDelete("{listId}/comment/{commentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteComment(
            [FromRoute] int clubId,
            [FromRoute] int listId,
            [FromRoute] int commentId)
        {
            var userId = GetCurrentUserId();
            await _service.DeleteCommentAsync(listId, commentId, userId);
            return NoContent();
        }
    }
}
