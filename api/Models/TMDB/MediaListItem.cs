using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TMDB
{
    public class MediaListItem
    {
        [Key]
        public int Id { get; set; }

        // Qual lista este item pertence?
        public int MediaListId { get; set; }
        [ForeignKey("MediaListId")]
        public MediaList MediaList { get; set; }

        // Qual é a obra (Filme/Série)?
        public int MediaItemId { get; set; }
        [ForeignKey("MediaItemId")]
        public MediaItem MediaItem { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Comentário opcional sobre o item na lista (Ex: "Melhor cena de ação")
        [MaxLength(500)]
        public string Note { get; set; }
    }
}