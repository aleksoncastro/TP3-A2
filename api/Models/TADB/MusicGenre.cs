using System.ComponentModel.DataAnnotations;

namespace MediaMatch.Models.TADB
{
    public class MusicGenre
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Ex: "Rock", "Jazz", "Hip-Hop"

        // Propriedade de Navegação para a tabela de ligação
        public ICollection<ArtistGenre> ArtistGenres { get; set; } = new List<ArtistGenre>();
    }
}