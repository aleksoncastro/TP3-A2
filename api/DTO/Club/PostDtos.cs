using System.ComponentModel.DataAnnotations;

namespace MediaMatch.DTO.Club
{
    // ========== POST DTOs ==========
    
    public class CreatePostDto
    {
        [Required(ErrorMessage = "Conteúdo é obrigatório")]
        [MaxLength(500, ErrorMessage = "Conteúdo não pode ter mais de 500 caracteres")]
        public string Content { get; set; } = string.Empty;
    }

    public class UpdatePostDto
    {
        [Required(ErrorMessage = "Conteúdo é obrigatório")]
        [MaxLength(500, ErrorMessage = "Conteúdo não pode ter mais de 500 caracteres")]
        public string Content { get; set; } = string.Empty;

        // Flag para remover imagem
        public bool RemoveImage { get; set; } = false;
    }

    public class PostDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        
        [Obsolete("Use ImageUrls instead")]
        public string? ImageUrl { get; set; }
        
        public List<string> ImageUrls { get; set; } = new();
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Relacionamentos
        public int ClubId { get; set; }
        public string ClubName { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePictureUrl { get; set; }
        
        // Contadores
        public int CommentsCount { get; set; }
        
        // Flags de estado
        public bool IsEdited => UpdatedAt.HasValue;
        
        // Permissões
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }

    public class PostDetailDto : PostDto
    {
        public List<CommentDto> Comments { get; set; } = new();
    }

    // ========== COMMENT DTOs ==========
    
    public class CreateCommentDto
    {
        [Required(ErrorMessage = "Conteúdo é obrigatório")]
        [MaxLength(1000, ErrorMessage = "Conteúdo não pode ter mais de 1000 caracteres")]
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateCommentDto
    {
        [Required(ErrorMessage = "Conteúdo é obrigatório")]
        [MaxLength(1000, ErrorMessage = "Conteúdo não pode ter mais de 1000 caracteres")]
        public string Content { get; set; } = string.Empty;
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Relacionamentos
        public int PostId { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePictureUrl { get; set; }
        
        // Permissões
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
