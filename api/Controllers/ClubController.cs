namespace MediaMatch.Controllers
{
    using MediaMatch.DTO.Club;
    using MediaMatch.Extensions;
    using MediaMatch.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;

    /// <summary>
    /// Endpoints para gerenciar clubes de mídia.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protege todos os endpoints por padrão
    [Produces("application/json")]
    public class ClubController : ControllerBase
    {
        private readonly ClubService _service;
        private readonly ILogger<ClubController> _logger;

        public ClubController(ClubService service, ILogger<ClubController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Extrai o ID do usuário autenticado do token JWT.
        /// </summary>
        private int GetCurrentUserId()
        {
            _logger.LogInformation("Tentando extrair userId do token. User.Identity.IsAuthenticated: {IsAuth}", User.Identity?.IsAuthenticated);
            
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Claim NameIdentifier encontrado: {Claim}", idClaim ?? "NULL");
            
            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var userId))
            {
                _logger.LogWarning("Token JWT inválido ou sem claim NameIdentifier. Claims disponíveis: {Claims}", 
                    string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                throw new UnauthorizedAccessException("Token inválido");
            }
            
            _logger.LogInformation("UserId extraído com sucesso: {UserId}", userId);
            return userId;
        }

        /// <summary>
        /// Tenta extrair o ID do usuário (retorna null se não autenticado).
        /// </summary>
        private int? TryGetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var userId))
                return null;
            return userId;
        }

        /// <summary>
        /// Cria um novo clube.
        /// </summary>
        /// <param name="dto">Dados do clube a ser criado.</param>
        /// <param name="image">Imagem do clube (opcional).</param>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ClubDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateClub([FromForm] CreateClubDto dto, IFormFile? image)
        {
            var userId = GetCurrentUserId();
            var club = await _service.CreateClubAsync(dto, userId, image);
            var clubDto = club.ToDto(userId);
            return CreatedAtAction(nameof(GetById), new { id = club.Id }, clubDto);
        }

        /// <summary>
        /// Lista todos os clubes com paginação e filtros.
        /// </summary>
        [HttpGet]
        [AllowAnonymous] // Permite buscar clubes públicos sem autenticação
        [ProducesResponseType(typeof(PagedResult<ClubDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] ClubFilterDto filter)
        {
            var currentUserId = TryGetCurrentUserId();
            var result = await _service.GetAllAsync(filter);
            
            var dtoResult = new PagedResult<ClubDto>
            {
                Items = result.Items.Select(c => c.ToDto(currentUserId)).ToList(),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
            
            return Ok(dtoResult);
        }

        /// <summary>
        /// Obtém um clube por ID com detalhes completos.
        /// </summary>
        /// <param name="id">ID do clube.</param>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ClubDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var currentUserId = TryGetCurrentUserId();
            var club = await _service.GetByIdAsync(id);
            var dto = club.ToDetailDto(currentUserId);
            return Ok(dto);
        }

        /// <summary>
        /// Atualiza um clube existente.
        /// </summary>
        /// <param name="id">ID do clube.</param>
        /// <param name="dto">Novos dados do clube.</param>
        /// <param name="image">Nova imagem do clube (opcional).</param>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ClubDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateClubDto dto, IFormFile? image)
        {
            var userId = GetCurrentUserId();
            var club = await _service.UpdateAsync(id, dto, userId, image);
            var clubDto = club.ToDto(userId);
            return Ok(clubDto);
        }

        /// <summary>
        /// Deleta um clube.
        /// </summary>
        /// <param name="id">ID do clube.</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            await _service.DeleteAsync(id, userId);
            return NoContent();
        }

        /// <summary>
        /// Adiciona um membro ao clube.
        /// </summary>
        /// <param name="clubId">ID do clube.</param>
        /// <param name="dto">Dados do membro a ser adicionado.</param>
        [HttpPost("{clubId}/members")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddMember(int clubId, [FromBody] AddMemberDto dto)
        {
            var requesterId = GetCurrentUserId();
            await _service.AddMemberAsync(clubId, dto.UserId, requesterId);
            return NoContent();
        }

        /// <summary>
        /// Remove um membro do clube (ou sai do clube se for o próprio membro).
        /// </summary>
        /// <param name="clubId">ID do clube.</param>
        /// <param name="userId">ID do usuário a ser removido.</param>
        [HttpDelete("{clubId}/members/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveMember(int clubId, int userId)
        {
            var requesterId = GetCurrentUserId();
            await _service.RemoveMemberAsync(clubId, userId, requesterId);
            return NoContent();
        }

        /// <summary>
        /// Promove ou remove status de moderador de um membro.
        /// </summary>
        /// <param name="clubId">ID do clube.</param>
        /// <param name="userId">ID do usuário.</param>
        [HttpPatch("{clubId}/members/{userId}/moderator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ToggleModerator(int clubId, int userId)
        {
            var requesterId = GetCurrentUserId();
            await _service.ToggleModeratorAsync(clubId, userId, requesterId);
            return NoContent();
        }

        /// <summary>
        /// Obtém todos os clubes do usuário autenticado (como owner ou membro).
        /// </summary>
        [HttpGet("my-clubs")]
        [ProducesResponseType(typeof(List<ClubDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyClubs()
        {
            try
            {
                _logger.LogInformation("=== GetMyClubs chamado ===");
                var userId = GetCurrentUserId();
                _logger.LogInformation("Buscando clubes para userId: {UserId}", userId);
                var clubs = await _service.GetUserClubsAsync(userId);
                _logger.LogInformation("Encontrados {Count} clubes para o usuário", clubs.Count);
                var dtos = clubs.Select(c => c.ToDto(userId)).ToList();
                return Ok(dtos);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Acesso não autorizado: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar clubes do usuário");
                return StatusCode(500, new { message = "Erro interno ao buscar clubes", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém a lista de membros de um clube.
        /// </summary>
        /// <param name="clubId">ID do clube.</param>
        [HttpGet("{clubId}/members")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ClubMemberDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClubMembers(int clubId)
        {
            var members = await _service.GetClubMembersAsync(clubId);
            var dtos = members.Select(m => new ClubMemberDto
            {
                UserId = m.UserId,
                UserName = m.User?.UserName,
                Email = m.User?.Email,
                JoinedAt = m.JoinedAt,
                IsModerator = m.IsModerator
            }).ToList();
            return Ok(dtos);
        }
    }

}

