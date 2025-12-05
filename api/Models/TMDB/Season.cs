using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TMDB
{
    public class Season
    {
        [Key]
        public int Id { get; set; }

        public int SeasonNumber { get; set; }

        [MaxLength(500)]
        public string PosterUrl { get; set; }

        public int MediaItemId { get; set; }
        [ForeignKey("MediaItemId")]
        public MediaItem MediaItem { get; set; }

        // Propriedade de Navegação
        public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
    }
}