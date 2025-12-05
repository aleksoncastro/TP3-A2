using Microsoft.AspNetCore.Mvc.Formatters;
using System.ComponentModel.DataAnnotations;

namespace MediaMatch.Models.TMDB
{
    public class MediaItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public MediaType Type { get; set; }

        public int TmdbId { get; set; }

        [MaxLength(500)]
        public string PosterUrl { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public float Rating { get; set; }

        // Propriedades de Navegação
        public ICollection<MediaGenre> MediaGenres { get; set; } = new List<MediaGenre>();
        public ICollection<Credit> Credits { get; set; } = new List<Credit>();
        public ICollection<Season> Seasons { get; set; } = new List<Season>();
    }
}
