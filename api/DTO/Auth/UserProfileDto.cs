namespace MediaMatch.DTO.Auth
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ProfilePictureUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; } = null!;
    }
}
