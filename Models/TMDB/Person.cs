using System.ComponentModel.DataAnnotations;

namespace MediaMatch.Models.TMDB
{
    public class Person
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public int TmdbPersonId { get; set; }

        [MaxLength(500)]
        public string ProfilePath { get; set; }

        // Propriedade de Navegação
        public ICollection<Credit> Credits { get; set; } = new List<Credit>();
    }
}
