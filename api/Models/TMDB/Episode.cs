using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TMDB
{
    public class Episode
    {
        [Key]
        public int Id { get; set; }

        public int EpisodeNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Overview { get; set; }

        public DateTime? AirDate { get; set; }

        public int SeasonId { get; set; }
        [ForeignKey("SeasonId")]
        public Season Season { get; set; }
    }
}