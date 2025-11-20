using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TMDB
{
    public class MediaList
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } // Ex: "Favoritos", "Para Assistir"

        public string Description { get; set; }

        public bool IsPublic { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // === DONO DA LISTA ===
        // Uma lista pode pertencer a um Usuário OU a um Clube

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int? ClubId { get; set; }
        [ForeignKey("ClubId")]
        public Club? Club { get; set; }

        // Itens da lista
        public ICollection<MediaListItem> Items { get; set; } = new List<MediaListItem>();
    }
}
