using System.ComponentModel.DataAnnotations;

namespace MediaMatch.Models

{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        public string HashedPassword { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propriedade de Navegação
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}

