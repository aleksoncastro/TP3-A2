using System.ComponentModel.DataAnnotations.Schema;

namespace MediaMatch.Models.TMDB
{
    public class ClubMember
    {
        public int ClubId { get; set; }
        [ForeignKey("ClubId")]
        public Club Club { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool IsModerator { get; set; } = false;
    }
}