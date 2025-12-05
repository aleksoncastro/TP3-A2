using System.ComponentModel.DataAnnotations;

namespace MediaMatch.DTO.Auth
{
    public class RegisterDto
    {
        [Required]
        [MinLength(3)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
    }

    public class RoleChangeRequestDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Role { get; set; }
    }

    public class UserListItemDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; }
    }

    public class UsersPagedResultDto
    {
        public List<UserListItemDto> Items { get; set; }
        public int Total { get; set; }
    }
}
