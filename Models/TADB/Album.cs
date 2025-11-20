using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TADB
{
    public class Album
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public int YearReleased { get; set; }

        [MaxLength(100)]
        public string Genre { get; set; }

        [MaxLength(200)]
        public string Label { get; set; }

        public int AudioDbAlbumId { get; set; }

        [MaxLength(500)]
        public string CoverUrl { get; set; }

        public int ArtistId { get; set; }
        [ForeignKey("ArtistId")]
        public Artist Artist { get; set; }

        // Propriedade de Navegação
        public ICollection<Track> Tracks { get; set; } = new List<Track>();
    }
}