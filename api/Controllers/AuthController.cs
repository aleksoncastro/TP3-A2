using MediaMatch.DTO.Auth;
using MediaMatch.Services;
using MediaMatch.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace MediaMatch.Controllers
{
    /// <summary>
    /// Endpoints de autenticação de usuários.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly MediaMatchContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, MediaMatchContext context, ILogger<AuthController> logger)
        {
            _authService = authService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Registra um novo usuário.
        /// </summary>
        /// <param name="dto">Dados de registro (email, senha, etc.).</param>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Realiza login e retorna token de acesso.
        /// </summary>
        /// <param name="dto">Credenciais de login.</param>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserListItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserListItemDto>> Me()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim)) return Unauthorized();
            if (!int.TryParse(idClaim, out var userId)) return Unauthorized();

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return Unauthorized();
            var roleName = user.UserRoles.FirstOrDefault()?.Role.Name ?? "user";
            if (string.Equals(roleName, "Membro", StringComparison.OrdinalIgnoreCase)) roleName = "user";

            return Ok(new UserListItemDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Role = roleName
            });
        }

        /// <summary>
        /// Obtém o perfil completo do usuário autenticado.
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var userId))
                return Unauthorized();

            try
            {
                var profile = await _authService.GetUserProfileAsync(userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Atualiza o perfil do usuário autenticado.
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var userId))
                return Unauthorized();

            try
            {
                var profile = await _authService.UpdateProfileAsync(userId, dto);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Atualiza a foto de perfil do usuário autenticado.
        /// </summary>
        [HttpPost("profile/picture")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateProfilePicture([FromForm] IFormFile file)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var userId))
                return Unauthorized();

            if (file == null || file.Length == 0)
                return BadRequest("Nenhum arquivo fornecido.");

            try
            {
                var imageUrl = await _authService.UpdateProfilePictureAsync(userId, file);
                return Ok(new { profilePictureUrl = imageUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("users")]
        [Authorize(Policy = "AdminOnly")]
        [EnableRateLimiting("rolesLimiter")]
        [ProducesResponseType(typeof(UsersPagedResultDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UsersPagedResultDto>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = null,
            [FromQuery] string? email = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null,
            [FromQuery] string? role = null)
        {
            var query = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name)) query = query.Where(u => u.UserName.Contains(name));
            if (!string.IsNullOrWhiteSpace(email)) query = query.Where(u => u.Email.Contains(email));
            if (createdFrom.HasValue) query = query.Where(u => u.CreatedAt >= createdFrom.Value);
            if (createdTo.HasValue) query = query.Where(u => u.CreatedAt <= createdTo.Value);
            if (!string.IsNullOrWhiteSpace(role))
            {
                var r = role.ToLower();
                if (r == "membro") r = "user";
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name.ToLower() == r));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    CreatedAt = u.CreatedAt,
                    Role = u.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault() ?? "user"
                })
                .ToListAsync();

            for (int i = 0; i < items.Count; i++)
            {
                if (string.Equals(items[i].Role, "Membro", StringComparison.OrdinalIgnoreCase)) items[i].Role = "user";
            }

            return Ok(new UsersPagedResultDto { Items = items, Total = total });
        }

        [HttpPut("roles")]
        [Authorize(Policy = "AdminOnly")]
        [EnableRateLimiting("rolesLimiter")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeRole(RoleChangeRequestDto dto)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim)) return Unauthorized();
            if (!int.TryParse(idClaim, out var requesterId)) return Unauthorized();

            var targetUser = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (targetUser == null) return BadRequest("Usuário não encontrado.");

            var newRole = dto.Role?.Trim().ToLower();
            if (string.IsNullOrEmpty(newRole)) return BadRequest("Role inválida.");
            if (newRole == "membro") newRole = "user";
            if (newRole != "user" && newRole != "admin") return BadRequest("Role inexistente.");

            if (requesterId == targetUser.Id && newRole != "admin") return BadRequest("Não é permitido remover seu próprio acesso de admin.");

            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == newRole);
            if (roleEntity == null)
            {
                roleEntity = new MediaMatch.Models.Role { Name = newRole };
                _context.Roles.Add(roleEntity);
                await _context.SaveChangesAsync();
            }

            var currentUserRole = targetUser.UserRoles.FirstOrDefault();
            if (currentUserRole == null)
            {
                _context.UserRoles.Add(new MediaMatch.Models.UserRole { UserId = targetUser.Id, RoleId = roleEntity.Id });
            }
            else
            {
                currentUserRole.RoleId = roleEntity.Id;
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role alterada: UserId={UserId} NewRole={Role} ChangedBy={ChangedBy}", targetUser.Id, newRole, requesterId);

            return NoContent();
        }

        [HttpDelete("users/{id:int}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim)) return Unauthorized();
            if (!int.TryParse(idClaim, out var requesterId)) return Unauthorized();

            try
            {
                await _authService.DeleteUserAsync(id, requesterId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
