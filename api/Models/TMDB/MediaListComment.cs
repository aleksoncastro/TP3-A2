using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TMDB
{
    public class MediaListComment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Tipo de comentário: "suggestion" para sugestões de mídia, "comment" para comentários gerais
        [Required]
        [MaxLength(20)]
        public string Type { get; set; } = "comment"; // "comment" ou "suggestion"

        // Se for sugestão, pode incluir informações da mídia sugerida
        public int? SuggestedMediaId { get; set; } // TMDB ID
        public string? SuggestedMediaType { get; set; } // "movie" ou "tv"
        public string? SuggestedMediaTitle { get; set; }
        public string? SuggestedMediaPosterPath { get; set; }

        // Relacionamentos
        public int MediaListId { get; set; }
        [ForeignKey("MediaListId")]
        public MediaList MediaList { get; set; } = null!;

        public int AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public User Author { get; set; } = null!;
    }
}
