namespace MediaMatch.DTO.Club
{
    // ===== MediaList DTOs =====
    
    public class MediaListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ClubId { get; set; }
        public string? ClubName { get; set; }
        public int ItemsCount { get; set; }
        public int CommentsCount { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class MediaListDetailDto : MediaListDto
    {
        public List<MediaListItemDto> Items { get; set; } = new();
        public List<MediaListCommentDto> Comments { get; set; } = new();
    }

    public class CreateMediaListDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = true;
    }

    public class UpdateMediaListDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
    }

    // ===== MediaListItem DTOs =====
    
    public class MediaListItemDto
    {
        public int Id { get; set; }
        public int MediaListId { get; set; }
        public int MediaItemId { get; set; }
        public DateTime AddedAt { get; set; }
        public string Note { get; set; } = string.Empty;
        
        // Informações da mídia
        public int TmdbId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? PosterPath { get; set; }
        public string MediaType { get; set; } = string.Empty; // "movie" ou "tv"
        public double? Rating { get; set; }
        public string? ReleaseDate { get; set; }
    }

    public class AddMediaListItemDto
    {
        public int TmdbId { get; set; }
        public string MediaType { get; set; } = string.Empty; // "movie" ou "tv"
        public string? Note { get; set; }
    }

    // ===== MediaListComment DTOs =====
    
    public class MediaListCommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Type { get; set; } = "comment";
        
        // Dados da sugestão (se aplicável)
        public int? SuggestedMediaId { get; set; }
        public string? SuggestedMediaType { get; set; }
        public string? SuggestedMediaTitle { get; set; }
        public string? SuggestedMediaPosterPath { get; set; }
        
        // Autor
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePictureUrl { get; set; }
        
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class CreateMediaListCommentDto
    {
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = "comment"; // "comment" ou "suggestion"
        
        // Se for sugestão, incluir dados da mídia
        public int? SuggestedMediaId { get; set; }
        public string? SuggestedMediaType { get; set; }
        public string? SuggestedMediaTitle { get; set; }
        public string? SuggestedMediaPosterPath { get; set; }
    }
}
