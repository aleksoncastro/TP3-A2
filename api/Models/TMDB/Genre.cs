using System.ComponentModel.DataAnnotations;

namespace MediaMatch.Models.TMDB
{
    public class Genre
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        // Propriedade de Navegação
        public ICollection<MediaGenre> MediaGenres { get; set; } = new List<MediaGenre>();
    }
}
