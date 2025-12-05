using MediaMatch.Data;
using MediaMatch.DTO.Club;
using MediaMatch.Exceptions;
using MediaMatch.Models.TMDB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediaMatch.Services
{
    public class ClubService
    {
        private readonly MediaMatchContext _context;
        private readonly ILogger<ClubService> _logger;
        private readonly FileUploadService _fileUploadService;

        public ClubService(
            MediaMatchContext context, 
            ILogger<ClubService> logger,
            FileUploadService fileUploadService)
        {
            _context = context;
            _logger = logger;
            _fileUploadService = fileUploadService;
        }

        public async Task<Club> CreateClubAsync(CreateClubDto dto, int ownerId, IFormFile? image)
        {
            try
            {
                // Validar se usuário existe
                var userExists = await _context.Users.AnyAsync(u => u.Id == ownerId);
                if (!userExists)
                    throw new NotFoundException("Usuário não encontrado");

                // Processar upload de imagem se fornecida
                string? imageUrl = null;
                if (image != null)
                {
                    imageUrl = await _fileUploadService.UploadImageAsync(image);
                }

                var club = new Club
                {
                    Name = dto.Name,
                    Description = dto.Description ?? string.Empty,
                    ImageUrl = imageUrl,
                    OwnerId = ownerId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Clubs.Add(club);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Clube {ClubName} criado por usuário {OwnerId}", dto.Name, ownerId);
                
                // Recarregar com relacionamentos
                return await GetByIdAsync(club.Id);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar clube");
                throw new DatabaseException("Erro ao criar clube", ex);
            }
        }

        public async Task<PagedResult<Club>> GetAllAsync(ClubFilterDto filter)
        {
            try
            {
                var query = _context.Clubs.AsQueryable();

                // Filtro por termo de busca
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    query = query.Where(c => 
                        c.Name.Contains(filter.SearchTerm) || 
                        c.Description.Contains(filter.SearchTerm));
                }

                // Filtro por dono
                if (filter.OwnerId.HasValue)
                {
                    query = query.Where(c => c.OwnerId == filter.OwnerId.Value);
                }

                // Ordenação
                query = ApplySorting(query, filter.SortBy, filter.SortOrder);

                // Total de registros
                var total = await query.CountAsync();

                // Paginação
                var clubs = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Include(c => c.Owner)
                    .Include(c => c.Members)
                        .ThenInclude(m => m.User)
                    .Include(c => c.MediaLists)
                    .ToListAsync();

                return new PagedResult<Club>
                {
                    Items = clubs,
                    TotalCount = total,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar clubes");
                throw new DatabaseException("Erro ao buscar clubes", ex);
            }
        }

        public async Task<Club> GetByIdAsync(int id)
        {
            try
            {
                var club = await _context.Clubs
                    .Include(c => c.Owner)
                    .Include(c => c.Members)
                        .ThenInclude(m => m.User)
                    .Include(c => c.MediaLists)
                        .ThenInclude(ml => ml.Items)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (club == null)
                    throw new NotFoundException($"Clube com ID {id} não encontrado");

                return club;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar clube {ClubId}", id);
                throw new DatabaseException("Erro ao buscar clube", ex);
            }
        }

        public async Task<Club> UpdateAsync(int id, UpdateClubDto dto, int userId, IFormFile? image)
        {
            try
            {
                var club = await _context.Clubs.FindAsync(id);
                if (club == null)
                    throw new NotFoundException($"Clube com ID {id} não encontrado");

                // Apenas owner pode atualizar
                if (club.OwnerId != userId)
                    throw new ForbiddenException("Apenas o dono pode atualizar o clube");

                // Atualizar dados básicos
                club.Name = dto.Name;
                club.Description = dto.Description ?? string.Empty;

                // Processar imagem
                if (dto.RemoveImage)
                {
                    // Remover imagem atual
                    if (!string.IsNullOrEmpty(club.ImageUrl))
                    {
                        await _fileUploadService.DeleteImageAsync(club.ImageUrl);
                        club.ImageUrl = null;
                    }
                }
                else if (image != null)
                {
                    // Fazer upload da nova imagem (remove a antiga automaticamente)
                    club.ImageUrl = await _fileUploadService.UploadImageAsync(image, club.ImageUrl);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Clube {ClubId} atualizado por usuário {UserId}", id, userId);

                // Recarregar com relacionamentos
                return await GetByIdAsync(id);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ForbiddenException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar clube {ClubId}", id);
                throw new DatabaseException("Erro ao atualizar clube", ex);
            }
        }

        public async Task DeleteAsync(int id, int userId)
        {
            try
            {
                var club = await _context.Clubs.FindAsync(id);
                if (club == null)
                    throw new NotFoundException($"Clube com ID {id} não encontrado");

                // Verifica se é owner ou admin do sistema
                var isSystemAdmin = await IsSystemAdminAsync(userId);
                if (club.OwnerId != userId && !isSystemAdmin)
                    throw new ForbiddenException("Apenas o dono ou administradores do sistema podem deletar o clube");

                // Deletar imagem associada
                if (!string.IsNullOrEmpty(club.ImageUrl))
                {
                    await _fileUploadService.DeleteImageAsync(club.ImageUrl);
                }

                _context.Clubs.Remove(club);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Clube {ClubId} deletado por usuário {UserId}", id, userId);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ForbiddenException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar clube {ClubId}", id);
                throw new DatabaseException("Erro ao deletar clube", ex);
            }
        }

        public async Task AddMemberAsync(int clubId, int userId, int requesterId)
        {
            try
            {
                var club = await _context.Clubs.FindAsync(clubId);
                if (club == null)
                    throw new NotFoundException("Clube não encontrado");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    throw new NotFoundException("Usuário não encontrado");

                // Owner não pode se adicionar como membro
                if (club.OwnerId == userId)
                    throw new BusinessException("O dono do clube não precisa ser adicionado como membro");

                // Verificar se já é membro
                var exists = await _context.ClubMembers
                    .AnyAsync(m => m.ClubId == clubId && m.UserId == userId);
                
                if (exists)
                    throw new BusinessException("Usuário já é membro deste clube");

                // Validar permissões:
                // 1. Se userId == requesterId: usuário está entrando no próprio clube (permitido)
                // 2. Caso contrário: apenas owner ou moderador podem adicionar outros usuários
                if (userId != requesterId)
                {
                    if (requesterId != club.OwnerId)
                    {
                        var isModerator = await _context.ClubMembers
                            .AnyAsync(m => m.ClubId == clubId && m.UserId == requesterId && m.IsModerator);
                        
                        if (!isModerator)
                            throw new ForbiddenException("Apenas o dono ou moderadores podem adicionar outros membros ao clube");
                    }
                }

                _context.ClubMembers.Add(new ClubMember
                {
                    ClubId = clubId,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow,
                    IsModerator = false
                });

                await _context.SaveChangesAsync();
                _logger.LogInformation("Usuário {UserId} adicionado ao clube {ClubId} por {RequesterId}", 
                    userId, clubId, requesterId);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (ForbiddenException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar membro ao clube {ClubId}", clubId);
                throw new DatabaseException("Erro ao adicionar membro", ex);
            }
        }

        public async Task RemoveMemberAsync(int clubId, int userId, int requesterId)
        {
            try
            {
                var club = await _context.Clubs.FindAsync(clubId);
                if (club == null)
                    throw new NotFoundException("Clube não encontrado");

                // Validar permissões
                // Pode remover: o próprio membro, o owner, ou um moderador
                if (requesterId != club.OwnerId && requesterId != userId)
                {
                    var isModerator = await _context.ClubMembers
                        .AnyAsync(m => m.ClubId == clubId && m.UserId == requesterId && m.IsModerator);
                    
                    if (!isModerator)
                        throw new ForbiddenException("Sem permissão para remover este membro");
                }

                var member = await _context.ClubMembers
                    .FirstOrDefaultAsync(m => m.ClubId == clubId && m.UserId == userId);
                
                if (member == null)
                    throw new NotFoundException("Membro não encontrado no clube");

                _context.ClubMembers.Remove(member);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Usuário {UserId} removido do clube {ClubId} por {RequesterId}", 
                    userId, clubId, requesterId);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ForbiddenException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover membro do clube {ClubId}", clubId);
                throw new DatabaseException("Erro ao remover membro", ex);
            }
        }

        public async Task ToggleModeratorAsync(int clubId, int userId, int requesterId)
        {
            try
            {
                var club = await _context.Clubs.FindAsync(clubId);
                if (club == null)
                    throw new NotFoundException("Clube não encontrado");

                // Apenas owner pode promover/remover moderadores
                if (club.OwnerId != requesterId)
                    throw new ForbiddenException("Apenas o dono pode gerenciar moderadores");

                var member = await _context.ClubMembers
                    .FirstOrDefaultAsync(m => m.ClubId == clubId && m.UserId == userId);
                
                if (member == null)
                    throw new NotFoundException("Membro não encontrado no clube");

                member.IsModerator = !member.IsModerator;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Status de moderador do usuário {UserId} no clube {ClubId} alterado para {IsModerator} por {RequesterId}", 
                    userId, clubId, member.IsModerator, requesterId);
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ForbiddenException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar status de moderador no clube {ClubId}", clubId);
                throw new DatabaseException("Erro ao alterar status de moderador", ex);
            }
        }

        public async Task<List<Club>> GetUserClubsAsync(int userId)
        {
            try
            {
                // Clubes onde o usuário é owner
                var ownedClubs = await _context.Clubs
                    .Where(c => c.OwnerId == userId)
                    .Include(c => c.Owner)
                    .Include(c => c.Members)
                    .Include(c => c.MediaLists)
                    .ToListAsync();

                // Clubes onde o usuário é membro (mas não owner)
                var memberClubIds = await _context.ClubMembers
                    .Where(m => m.UserId == userId)
                    .Select(m => m.ClubId)
                    .ToListAsync();

                var memberClubs = await _context.Clubs
                    .Where(c => memberClubIds.Contains(c.Id))
                    .Include(c => c.Owner)
                    .Include(c => c.Members)
                    .Include(c => c.MediaLists)
                    .ToListAsync();

                // Combinar e remover duplicatas (caso seja owner e membro ao mesmo tempo)
                var allClubs = ownedClubs
                    .Union(memberClubs)
                    .DistinctBy(c => c.Id)
                    .ToList();
                
                return allClubs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar clubes do usuário {UserId}", userId);
                throw new DatabaseException("Erro ao buscar clubes do usuário", ex);
            }
        }

        public async Task<List<ClubMember>> GetClubMembersAsync(int clubId)
        {
            try
            {
                var clubExists = await _context.Clubs.AnyAsync(c => c.Id == clubId);
                if (!clubExists)
                    throw new NotFoundException("Clube não encontrado");

                return await _context.ClubMembers
                    .Where(m => m.ClubId == clubId)
                    .Include(m => m.User)
                    .OrderByDescending(m => m.IsModerator)
                    .ThenBy(m => m.JoinedAt)
                    .ToListAsync();
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar membros do clube {ClubId}", clubId);
                throw new DatabaseException("Erro ao buscar membros do clube", ex);
            }
        }

        public async Task<bool> IsOwnerAsync(int clubId, int userId)
        {
            var club = await _context.Clubs.FindAsync(clubId);
            return club != null && club.OwnerId == userId;
        }

        public async Task<bool> IsMemberAsync(int clubId, int userId)
        {
            return await _context.ClubMembers
                .AnyAsync(m => m.ClubId == clubId && m.UserId == userId);
        }

        public async Task<bool> IsModeratorAsync(int clubId, int userId)
        {
            var member = await _context.ClubMembers
                .FirstOrDefaultAsync(m => m.ClubId == clubId && m.UserId == userId);
            
            return member != null && member.IsModerator;
        }

        private IQueryable<Club> ApplySorting(IQueryable<Club> query, string? sortBy, string? sortOrder)
        {
            var isDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            return sortBy?.ToLower() switch
            {
                "oldest" => isDescending 
                    ? query.OrderByDescending(c => c.CreatedAt) 
                    : query.OrderBy(c => c.CreatedAt),
                "name" => isDescending 
                    ? query.OrderByDescending(c => c.Name) 
                    : query.OrderBy(c => c.Name),
                "members" => isDescending 
                    ? query.OrderByDescending(c => c.Members.Count) 
                    : query.OrderBy(c => c.Members.Count),
                "newest" or _ => isDescending 
                    ? query.OrderByDescending(c => c.CreatedAt) 
                    : query.OrderBy(c => c.CreatedAt)
            };
        }

        private async Task<bool> IsSystemAdminAsync(int userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == "admin");
        }
    }
}
