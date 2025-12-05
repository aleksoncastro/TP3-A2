using MediaMatch.Models.TMDB;
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

        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [MaxLength(200)]
        public string? PasswordResetCodeHash { get; set; }

        public DateTime? PasswordResetCodeExpiresAt { get; set; }

        // Propriedades de Navegação
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<ClubMember> ClubMemberships { get; set; } = new List<ClubMember>();
        public ICollection<MediaList> MediaLists { get; set; } = new List<MediaList>();
    }
}

