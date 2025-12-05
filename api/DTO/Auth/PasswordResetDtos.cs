using System.ComponentModel.DataAnnotations;

namespace MediaMatch.DTO.Auth
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(6)]
        public string Code { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
    }
}
