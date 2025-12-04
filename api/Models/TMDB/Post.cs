using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TMDB
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        [Obsolete("Use Images collection instead")]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Relacionamentos
        public int ClubId { get; set; }
        [ForeignKey("ClubId")]
        public Club Club { get; set; } = null!;

        public int AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public User Author { get; set; } = null!;

        // Um post pode ter vários comentários
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Um post pode ter várias imagens
        public ICollection<PostImage> Images { get; set; } = new List<PostImage>();
    }
}
