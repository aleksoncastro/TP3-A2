using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TMDB
{
    public class Club
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // O Dono/Criador do clube
        public int OwnerId { get; set; }
        [ForeignKey("OwnerId")]
        public User Owner { get; set; }

        // Relação com Membros
        public ICollection<ClubMember> Members { get; set; } = new List<ClubMember>();

        // Um clube pode ter várias listas de recomendação
        public ICollection<MediaList> MediaLists { get; set; } = new List<MediaList>();
    }
}
