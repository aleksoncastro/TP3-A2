using Microsoft.EntityFrameworkCore;
using MediaMatch.Data;
using MediaMatch.Models.TMDB;
using MediaMatch.DTO.Club;
using MediaMatch.Exceptions;

namespace MediaMatch.Services
{
    public class MediaListService
    {
        private readonly MediaMatchContext _context;
        private readonly TmdbService _tmdbService;
        private readonly ILogger<MediaListService> _logger;

        public MediaListService(
            MediaMatchContext context,
            TmdbService tmdbService,
            ILogger<MediaListService> logger)
        {
            _context = context;
            _tmdbService = tmdbService;
            _logger = logger;
        }

        // ===== CRUD de MediaList =====

        public async Task<MediaList> CreateListAsync(int clubId, CreateMediaListDto dto, int userId)
        {
            try
            {
                // Verificar se o clube existe
                var club = await _context.Clubs.FindAsync(clubId);
                if (club == null)
                    throw new NotFoundException($"Clube com ID {clubId} não encontrado");

                // Verificar se o usuário é membro do clube, owner ou admin do sistema
                var isSystemAdmin = await IsSystemAdminAsync(userId);
                var member = await _context.ClubMembers
                    .FirstOrDefaultAsync(cm => cm.ClubId == clubId && cm.UserId == userId);

                // Permite: admin do sistema, owner do clube, ou qualquer membro do clube
                if (!isSystemAdmin && club.OwnerId != userId && member == null)
                    throw new ForbiddenException("Você precisa ser membro do clube para criar listas");

                var mediaList = new MediaList
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    IsPublic = dto.IsPublic,
                    ClubId = clubId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MediaLists.Add(mediaList);
                await _context.SaveChangesAsync();

                return mediaList;
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException)
            {
                _logger.LogError(ex, "Erro ao criar lista");
                throw new DatabaseException("Erro ao criar lista", ex);
            }
        }

        public async Task<List<MediaListDto>> GetClubListsAsync(int clubId, int currentUserId)
        {
            var lists = await _context.MediaLists
                .Include(ml => ml.Items)
                .Include(ml => ml.Comments)
                .Include(ml => ml.Club)
                .Where(ml => ml.ClubId == clubId)
                .OrderByDescending(ml => ml.CreatedAt)
                .ToListAsync();

            var club = await _context.Clubs.FindAsync(clubId);
            var member = await _context.ClubMembers
                .FirstOrDefaultAsync(cm => cm.ClubId == clubId && cm.UserId == currentUserId);
            var isSystemAdmin = await IsSystemAdminAsync(currentUserId);

            return lists.Select(ml => new MediaListDto
            {
                Id = ml.Id,
                Name = ml.Name,
                Description = ml.Description,
                IsPublic = ml.IsPublic,
                CreatedAt = ml.CreatedAt,
                ClubId = ml.ClubId,
                ClubName = ml.Club?.Name,
                ItemsCount = ml.Items?.Count ?? 0,
                CommentsCount = ml.Comments?.Count ?? 0,
                CanEdit = isSystemAdmin || (club != null && (club.OwnerId == currentUserId || (member?.IsModerator ?? false))),
                CanDelete = isSystemAdmin || (club != null && (club.OwnerId == currentUserId || (member?.IsModerator ?? false)))
            }).ToList();
        }

        public async Task<MediaListDetailDto?> GetListDetailAsync(int listId, int currentUserId)
        {
            var mediaList = await _context.MediaLists
                .Include(ml => ml.Items)
                    .ThenInclude(item => item.MediaItem)
                .Include(ml => ml.Comments)
                    .ThenInclude(c => c.Author)
                .Include(ml => ml.Club)
                .FirstOrDefaultAsync(ml => ml.Id == listId);

            if (mediaList == null)
                return null;

            var club = mediaList.Club;
            var member = club != null ? await _context.ClubMembers
                .FirstOrDefaultAsync(cm => cm.ClubId == club.Id && cm.UserId == currentUserId) : null;
            var isSystemAdmin = await IsSystemAdminAsync(currentUserId);

            bool canEdit = isSystemAdmin || (club != null && (club.OwnerId == currentUserId || (member?.IsModerator ?? false)));

            return new MediaListDetailDto
            {
                Id = mediaList.Id,
                Name = mediaList.Name,
                Description = mediaList.Description,
                IsPublic = mediaList.IsPublic,
                CreatedAt = mediaList.CreatedAt,
                ClubId = mediaList.ClubId,
                ClubName = club?.Name,
                ItemsCount = mediaList.Items?.Count ?? 0,
                CommentsCount = mediaList.Comments?.Count ?? 0,
                CanEdit = canEdit,
                CanDelete = canEdit,
                Items = mediaList.Items?.Select(item => new MediaListItemDto
                {
                    Id = item.Id,
                    MediaListId = item.MediaListId,
                    MediaItemId = item.MediaItemId,
                    AddedAt = item.AddedAt,
                    Note = item.Note ?? string.Empty,
                    TmdbId = item.MediaItem.TmdbId,
                    Title = item.MediaItem.Title,
                    PosterPath = item.MediaItem.PosterUrl,
                    MediaType = item.MediaItem.Type == MediaType.Movie ? "movie" : "tv",
                    Rating = item.MediaItem.Rating,
                    ReleaseDate = item.MediaItem.ReleaseDate?.ToString("yyyy-MM-dd")
                }).ToList() ?? new(),
                Comments = mediaList.Comments?.Select(c => new MediaListCommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    Type = c.Type,
                    SuggestedMediaId = c.SuggestedMediaId,
                    SuggestedMediaType = c.SuggestedMediaType,
                    SuggestedMediaTitle = c.SuggestedMediaTitle,
                    SuggestedMediaPosterPath = c.SuggestedMediaPosterPath,
                    AuthorId = c.AuthorId,
                    AuthorName = c.Author.UserName,
                    AuthorProfilePictureUrl = c.Author.ProfilePictureUrl,
                    CanEdit = c.AuthorId == currentUserId,
                    CanDelete = c.AuthorId == currentUserId || canEdit
                }).ToList() ?? new()
            };
        }

        public async Task<MediaList> UpdateListAsync(int listId, UpdateMediaListDto dto, int userId)
        {
            try
            {
                var mediaList = await _context.MediaLists
                    .Include(ml => ml.Club)
                    .FirstOrDefaultAsync(ml => ml.Id == listId);

                if (mediaList == null)
                    throw new NotFoundException($"Lista com ID {listId} não encontrada");

                var isSystemAdmin = await IsSystemAdminAsync(userId);
                var club = mediaList.Club;
                if (club != null && !isSystemAdmin)
                {
                    var member = await _context.ClubMembers
                        .FirstOrDefaultAsync(cm => cm.ClubId == club.Id && cm.UserId == userId);

                    if (club.OwnerId != userId && !(member?.IsModerator ?? false))
                        throw new ForbiddenException("Apenas administradores podem editar listas");
                }

                mediaList.Name = dto.Name;
                mediaList.Description = dto.Description;
                mediaList.IsPublic = dto.IsPublic;

                await _context.SaveChangesAsync();
                return mediaList;
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException)
            {
                _logger.LogError(ex, "Erro ao atualizar lista");
                throw new DatabaseException("Erro ao atualizar lista", ex);
            }
        }

        public async Task DeleteListAsync(int listId, int userId)
        {
            try
            {
                var mediaList = await _context.MediaLists
                    .Include(ml => ml.Club)
                    .FirstOrDefaultAsync(ml => ml.Id == listId);

                if (mediaList == null)
                    throw new NotFoundException($"Lista com ID {listId} não encontrada");

                var isSystemAdmin = await IsSystemAdminAsync(userId);
                var club = mediaList.Club;
                if (club != null && !isSystemAdmin)
                {
                    var member = await _context.ClubMembers
                        .FirstOrDefaultAsync(cm => cm.ClubId == club.Id && cm.UserId == userId);

                    if (club.OwnerId != userId && !(member?.IsModerator ?? false))
                        throw new ForbiddenException("Apenas administradores podem deletar listas");
                }

                _context.MediaLists.Remove(mediaList);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException)
            {
                _logger.LogError(ex, "Erro ao deletar lista");
                throw new DatabaseException("Erro ao deletar lista", ex);
            }
        }

        // ===== Gerenciamento de Itens =====

        public async Task<MediaListItem> AddItemToListAsync(int listId, AddMediaListItemDto dto, int userId)
        {
            try
            {
                var mediaList = await _context.MediaLists
                    .Include(ml => ml.Club)
                    .FirstOrDefaultAsync(ml => ml.Id == listId);

                if (mediaList == null)
                    throw new NotFoundException($"Lista com ID {listId} não encontrada");

                // Verificar permissões: admin do sistema, owner do clube, ou membro do clube
                var isSystemAdmin = await IsSystemAdminAsync(userId);
                var club = mediaList.Club;
                if (club != null && !isSystemAdmin)
                {
                    var member = await _context.ClubMembers
                        .FirstOrDefaultAsync(cm => cm.ClubId == club.Id && cm.UserId == userId);

                    if (club.OwnerId != userId && member == null)
                        throw new ForbiddenException("Você precisa ser membro do clube para adicionar itens");
                }

                // Buscar ou criar MediaItem
                var mediaType = dto.MediaType == "movie" ? MediaType.Movie : MediaType.Series;
                var mediaItem = await _context.MediaItems
                    .FirstOrDefaultAsync(mi => mi.TmdbId == dto.TmdbId && mi.Type == mediaType);

                if (mediaItem == null)
                {
                    // Buscar dados do TMDB
                    if (dto.MediaType == "movie")
                    {
                        var movieDetailsJson = await _tmdbService.GetMovieDetailsAsync(dto.TmdbId);
                        var movieDetails = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(movieDetailsJson);
                        
                        mediaItem = new MediaItem
                        {
                            TmdbId = dto.TmdbId,
                            Type = MediaType.Movie,
                            Title = movieDetails.TryGetProperty("title", out var title) ? title.GetString() ?? string.Empty : string.Empty,
                            Description = movieDetails.TryGetProperty("overview", out var overview) ? overview.GetString() ?? string.Empty : string.Empty,
                            PosterUrl = movieDetails.TryGetProperty("poster_path", out var posterPath) ? posterPath.GetString() ?? string.Empty : string.Empty,
                            ReleaseDate = movieDetails.TryGetProperty("release_date", out var releaseDate) && !string.IsNullOrEmpty(releaseDate.GetString()) 
                                ? DateTime.Parse(releaseDate.GetString()!) : null,
                            Rating = movieDetails.TryGetProperty("vote_average", out var voteAverage) ? (float)voteAverage.GetDouble() : 0
                        };
                    }
                    else if (dto.MediaType == "tv")
                    {
                        var tvDetailsJson = await _tmdbService.GetSeriesDetailsAsync(dto.TmdbId);
                        var tvDetails = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(tvDetailsJson);
                        
                        mediaItem = new MediaItem
                        {
                            TmdbId = dto.TmdbId,
                            Type = MediaType.Series,
                            Title = tvDetails.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
                            Description = tvDetails.TryGetProperty("overview", out var overview) ? overview.GetString() ?? string.Empty : string.Empty,
                            PosterUrl = tvDetails.TryGetProperty("poster_path", out var posterPath) ? posterPath.GetString() ?? string.Empty : string.Empty,
                            ReleaseDate = tvDetails.TryGetProperty("first_air_date", out var firstAirDate) && !string.IsNullOrEmpty(firstAirDate.GetString()) 
                                ? DateTime.Parse(firstAirDate.GetString()!) : null,
                            Rating = tvDetails.TryGetProperty("vote_average", out var voteAverage) ? (float)voteAverage.GetDouble() : 0
                        };
                    }
                    else
                    {
                        throw new BusinessException("Tipo de mídia inválido");
                    }

                    _context.MediaItems.Add(mediaItem);
                    await _context.SaveChangesAsync();
                }

                // Verificar se já existe na lista
                var existingItem = await _context.MediaListItems
                    .FirstOrDefaultAsync(item => item.MediaListId == listId && item.MediaItemId == mediaItem.Id);

                if (existingItem != null)
                    throw new BusinessException("Item já existe na lista");

                var listItem = new MediaListItem
                {
                    MediaListId = listId,
                    MediaItemId = mediaItem.Id,
                    Note = dto.Note ?? string.Empty,
                    AddedAt = DateTime.UtcNow
                };

                _context.MediaListItems.Add(listItem);
                await _context.SaveChangesAsync();

                // Carregar relacionamentos
                await _context.Entry(listItem)
                    .Reference(li => li.MediaItem)
                    .LoadAsync();

                return listItem;
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException && ex is not BusinessException)
            {
                _logger.LogError(ex, "Erro ao adicionar item à lista");
                throw new DatabaseException("Erro ao adicionar item à lista", ex);
            }
        }

        public async Task RemoveItemFromListAsync(int listId, int itemId, int userId)
        {
            try
            {
                var mediaList = await _context.MediaLists
                    .Include(ml => ml.Club)
                    .FirstOrDefaultAsync(ml => ml.Id == listId);

                if (mediaList == null)
                    throw new NotFoundException($"Lista com ID {listId} não encontrada");

                var club = mediaList.Club;
                if (club != null)
                {
                    var member = await _context.ClubMembers
                        .FirstOrDefaultAsync(cm => cm.ClubId == club.Id && cm.UserId == userId);

                    if (club.OwnerId != userId && !(member?.IsModerator ?? false))
                        throw new ForbiddenException("Apenas administradores podem remover itens");
                }

                var item = await _context.MediaListItems
                    .FirstOrDefaultAsync(item => item.Id == itemId && item.MediaListId == listId);

                if (item == null)
                    throw new NotFoundException("Item não encontrado na lista");

                _context.MediaListItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException)
            {
                _logger.LogError(ex, "Erro ao remover item da lista");
                throw new DatabaseException("Erro ao remover item da lista", ex);
            }
        }

        // ===== Comentários =====

        public async Task<MediaListComment> CreateCommentAsync(int listId, CreateMediaListCommentDto dto, int userId)
        {
            try
            {
                var mediaList = await _context.MediaLists
                    .Include(ml => ml.Club)
                    .FirstOrDefaultAsync(ml => ml.Id == listId);

                if (mediaList == null)
                    throw new NotFoundException($"Lista com ID {listId} não encontrada");

                // Verificar se é membro, dono do clube ou admin do sistema (para comentar)
                var isSystemAdmin = await IsSystemAdminAsync(userId);
                
                if (!isSystemAdmin)
                {
                    var club = mediaList.Club;
                    if (club != null)
                    {
                        var isOwner = club.OwnerId == userId;
                        var isMember = await _context.ClubMembers
                            .AnyAsync(cm => cm.ClubId == club.Id && cm.UserId == userId);

                        if (!isOwner && !isMember)
                            throw new ForbiddenException("Apenas membros podem comentar");
                    }
                }

                var comment = new MediaListComment
                {
                    Content = dto.Content,
                    Type = dto.Type,
                    SuggestedMediaId = dto.SuggestedMediaId,
                    SuggestedMediaType = dto.SuggestedMediaType,
                    SuggestedMediaTitle = dto.SuggestedMediaTitle,
                    SuggestedMediaPosterPath = dto.SuggestedMediaPosterPath,
                    MediaListId = listId,
                    AuthorId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MediaListComments.Add(comment);
                await _context.SaveChangesAsync();

                await _context.Entry(comment)
                    .Reference(c => c.Author)
                    .LoadAsync();

                return comment;
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException)
            {
                _logger.LogError(ex, "Erro ao criar comentário");
                throw new DatabaseException("Erro ao criar comentário", ex);
            }
        }

        public async Task DeleteCommentAsync(int listId, int commentId, int userId)
        {
            try
            {
                var comment = await _context.MediaListComments
                    .Include(c => c.MediaList)
                        .ThenInclude(ml => ml!.Club)
                    .FirstOrDefaultAsync(c => c.Id == commentId && c.MediaListId == listId);

                if (comment == null)
                    throw new NotFoundException("Comentário não encontrado");

                var club = comment.MediaList.Club;
                var isAdmin = false;

                if (club != null)
                {
                    var member = await _context.ClubMembers
                        .FirstOrDefaultAsync(cm => cm.ClubId == club.Id && cm.UserId == userId);
                    isAdmin = club.OwnerId == userId || (member?.IsModerator ?? false);
                }

                if (comment.AuthorId != userId && !isAdmin)
                    throw new ForbiddenException("Sem permissão para deletar este comentário");

                _context.MediaListComments.Remove(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException)
            {
                _logger.LogError(ex, "Erro ao deletar comentário");
                throw new DatabaseException("Erro ao deletar comentário", ex);
            }
        }

        private async Task<bool> IsSystemAdminAsync(int userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == "admin");
        }
    }
}
