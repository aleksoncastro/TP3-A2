using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TADB
{
    public class Track
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public int Duration { get; set; } // em segundos

        public int AudioDbTrackId { get; set; }

        [MaxLength(500)]
        public string PreviewUrl { get; set; }

        public int AlbumId { get; set; }
        [ForeignKey("AlbumId")]
        public Album Album { get; set; }

        // Propriedade de Navegação para a tabela de ligação
        public ICollection<MediaSoundtrack> MediaSoundtracks { get; set; } = new List<MediaSoundtrack>();
    }
}