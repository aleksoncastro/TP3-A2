using System.ComponentModel.DataAnnotations;

namespace MediaMatch.Models.TADB
{
    public class Artist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        // REMOVIDO: public string Genre { get; set; } 
        // Substituído pela relação abaixo:

        public string Biography { get; set; }

        [MaxLength(100)]
        public string Country { get; set; }

        public int AudioDbArtistId { get; set; }

        [MaxLength(500)]
        public string ThumbnailUrl { get; set; }

        // Relação com Álbuns
        public ICollection<Album> Albums { get; set; } = new List<Album>();

        // NOVA relação com Gêneros Musicais
        public ICollection<ArtistGenre> ArtistGenres { get; set; } = new List<ArtistGenre>();
    }
}
