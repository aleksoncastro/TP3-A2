using System.ComponentModel.DataAnnotations;

namespace MediaMatch.DTO.Auth
{
    public class UpdateProfileDto
    {
        [MaxLength(100)]
        public string? UserName { get; set; }

        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }
    }
}
