using MediaMatch.Models.TMDB;
using MediaMatch.DTO.Club;

namespace MediaMatch.Extensions
{
    public static class PostExtensions
    {
        public static PostDto ToDto(this Post post, int currentUserId, bool isSystemAdmin = false, bool isClubOwner = false, bool isClubModerator = false)
        {
            var imageUrls = post.Images?
                .OrderBy(i => i.Order)
                .Select(i => i.ImageUrl)
                .ToList() ?? new List<string>();
            
#pragma warning disable CS0618
            // Fallback para compatibilidade com ImageUrl antigo
            if (!imageUrls.Any() && !string.IsNullOrEmpty(post.ImageUrl))
            {
                imageUrls.Add(post.ImageUrl);
            }
#pragma warning restore CS0618

            var isAuthor = post.AuthorId == currentUserId;
            var canDelete = isAuthor || isSystemAdmin || isClubOwner || isClubModerator;

            return new PostDto
            {
                Id = post.Id,
                Content = post.Content,
#pragma warning disable CS0618
                ImageUrl = imageUrls.FirstOrDefault(),
#pragma warning restore CS0618
                ImageUrls = imageUrls,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                ClubId = post.ClubId,
                ClubName = post.Club?.Name ?? string.Empty,
                AuthorId = post.AuthorId,
                AuthorName = post.Author?.UserName ?? string.Empty,
                AuthorProfilePictureUrl = post.Author?.ProfilePictureUrl,
                CommentsCount = post.Comments?.Count ?? 0,
                CanEdit = isAuthor,
                CanDelete = canDelete
            };
        }

        public static PostDetailDto ToDetailDto(this Post post, int currentUserId, bool isSystemAdmin = false, bool isClubOwner = false, bool isClubModerator = false)
        {
            var imageUrls = post.Images?
                .OrderBy(i => i.Order)
                .Select(i => i.ImageUrl)
                .ToList() ?? new List<string>();
            
#pragma warning disable CS0618
            // Fallback para compatibilidade com ImageUrl antigo
            if (!imageUrls.Any() && !string.IsNullOrEmpty(post.ImageUrl))
            {
                imageUrls.Add(post.ImageUrl);
            }
#pragma warning restore CS0618

            var isAuthor = post.AuthorId == currentUserId;
            var canDelete = isAuthor || isSystemAdmin || isClubOwner || isClubModerator;

            return new PostDetailDto
            {
                Id = post.Id,
                Content = post.Content,
#pragma warning disable CS0618
                ImageUrl = imageUrls.FirstOrDefault(),
#pragma warning restore CS0618
                ImageUrls = imageUrls,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                ClubId = post.ClubId,
                ClubName = post.Club?.Name ?? string.Empty,
                AuthorId = post.AuthorId,
                AuthorName = post.Author?.UserName ?? string.Empty,
                AuthorProfilePictureUrl = post.Author?.ProfilePictureUrl,
                CommentsCount = post.Comments?.Count ?? 0,
                CanEdit = isAuthor,
                CanDelete = canDelete,
                Comments = post.Comments?.Select(c => c.ToDto(currentUserId, isSystemAdmin, isClubOwner, isClubModerator)).ToList() ?? new()
            };
        }
    }

    public static class CommentExtensions
    {
        public static CommentDto ToDto(this Comment comment, int currentUserId, bool isSystemAdmin = false, bool isClubOwner = false, bool isClubModerator = false)
        {
            var isAuthor = comment.AuthorId == currentUserId;
            var canDelete = isAuthor || isSystemAdmin || isClubOwner || isClubModerator;

            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                PostId = comment.PostId,
                AuthorId = comment.AuthorId,
                AuthorName = comment.Author?.UserName ?? string.Empty,
                AuthorProfilePictureUrl = comment.Author?.ProfilePictureUrl,
                CanEdit = isAuthor,
                CanDelete = canDelete
            };
        }
    }
}
