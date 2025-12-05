using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TMDB
{
    public class Credit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Role { get; set; } // Ex: Director, Actor, Writer

        [MaxLength(200)]
        public string CharacterName { get; set; }

        public int MediaItemId { get; set; }
        [ForeignKey("MediaItemId")]
        public MediaItem MediaItem { get; set; }

        public int PersonId { get; set; }
        [ForeignKey("PersonId")]
        public Person Person { get; set; }
    }
}