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
    }

    public class AuthService : IAuthService
    {
        private readonly MediaMatchContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(MediaMatchContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            // 4. Atribui a Role (Ex: Membro, Admin)
            // Nota: As Roles já devem existir no banco (Seed)
            var roleName = string.IsNullOrEmpty(dto.Role) ? "Membro" : dto.Role;
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
            {
                // Se a role não existe, cria uma padrão ou lança erro.
                // Aqui vamos criar para facilitar o teste, mas em prod ideal é lançar erro.
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

            // 5. Retorna o Token
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

            // 3. Pega a Role principal (assumindo 1 role por user para simplificar o login)
            var roleName = user.UserRoles.FirstOrDefault()?.Role.Name ?? "Membro";

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
                new Claim(ClaimTypes.Role, role) // Importante para o [Authorize(Roles="Admin")]
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
    }
}
