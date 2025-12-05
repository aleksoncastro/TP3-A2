using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace MediaMatch.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
