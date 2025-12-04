using MediaMatch.Data;
using MediaMatch.DTO.Auth;
using MediaMatch.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MediaMatch.Services
{
    
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<string> UpdateProfilePictureAsync(int userId, IFormFile file);
    }

    public class AuthService : IAuthService
    {
        private readonly MediaMatchContext _context;
        private readonly IConfiguration _configuration;
        private readonly FileUploadService _fileUploadService;

        public AuthService(MediaMatchContext context, IConfiguration configuration, FileUploadService fileUploadService)
        {
            _context = context;
            _configuration = configuration;
            _fileUploadService = fileUploadService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // 1. Verifica se email já existe
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                throw new Exception("Email já cadastrado.");
            }

            // 2. Criptografa a senha
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 3. Cria o Usuário
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                HashedPassword = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var roleName = "user";
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
            {
                role = new Role { Name = roleName };
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
            }

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            return GenerateToken(user, role.Name);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // 1. Busca usuário
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                throw new Exception("Usuário ou senha inválidos.");
            }

            // 2. Verifica senha
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
            {
                throw new Exception("Usuário ou senha inválidos.");
            }

            var roleName = user.UserRoles.FirstOrDefault()?.Role.Name ?? "user";
            roleName = NormalizeRoleName(roleName);

            // 4. Gera Token
            return GenerateToken(user, roleName);
        }

        private AuthResponseDto GenerateToken(User user, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthResponseDto
            {
                Token = tokenString,
                UserName = user.UserName,
                Role = role
            };
        }

        private string NormalizeRoleName(string name)
        {
            if (string.Equals(name, "Membro", StringComparison.OrdinalIgnoreCase)) return "user";
            return name;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("Usuário não encontrado.");

            var roleName = user.UserRoles.FirstOrDefault()?.Role.Name ?? "user";
            roleName = NormalizeRoleName(roleName);

            return new UserProfileDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                Role = roleName
            };
        }

        public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("Usuário não encontrado.");

            // Atualiza apenas campos fornecidos
            if (!string.IsNullOrWhiteSpace(dto.UserName))
                user.UserName = dto.UserName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                // Verifica se email já está em uso
                var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userId);
                if (emailExists)
                    throw new Exception("Email já cadastrado.");
                
                user.Email = dto.Email;
            }

            if (dto.PhoneNumber != null)
                user.PhoneNumber = dto.PhoneNumber;

            if (dto.Bio != null)
                user.Bio = dto.Bio;

            await _context.SaveChangesAsync();

            return await GetUserProfileAsync(userId);
        }

        public async Task<string> UpdateProfilePictureAsync(int userId, IFormFile file)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("Usuário não encontrado.");

            // Remove imagem antiga se existir e faz upload da nova
            var imageUrl = await _fileUploadService.UploadImageAsync(file, "avatars", user.ProfilePictureUrl);
            user.ProfilePictureUrl = imageUrl;

            await _context.SaveChangesAsync();

            return imageUrl;
        }
    }
}
