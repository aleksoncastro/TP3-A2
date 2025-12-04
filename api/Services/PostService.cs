using Microsoft.EntityFrameworkCore;
using MediaMatch.Data;
using MediaMatch.Models.TMDB;
using MediaMatch.DTO.Club;
using MediaMatch.Exceptions;
using MediaMatch.Extensions;

namespace MediaMatch.Services
{
    public class PostService
    {
        private readonly MediaMatchContext _context;
        private readonly FileUploadService _fileUploadService;
        private readonly ILogger<PostService> _logger;

        public PostService(MediaMatchContext context, FileUploadService fileUploadService, ILogger<PostService> logger)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        public async Task<Post> CreatePostAsync(int clubId, CreatePostDto dto, int authorId, List<IFormFile> images)
        {
            try
            {
                // Verificar se o clube existe
                var club = await _context.Clubs.FindAsync(clubId);
                if (club == null)
                    throw new NotFoundException($"Clube com ID {clubId} não encontrado");

                // Verificar se o usuário é membro do clube
                var isMember = await _context.ClubMembers
                    .AnyAsync(cm => cm.ClubId == clubId && cm.UserId == authorId);

                if (!isMember)
                    throw new ForbiddenException("Apenas membros podem criar posts no clube");

                var post = new Post
                {
                    Content = dto.Content,
                    ClubId = clubId,
                    AuthorId = authorId,
                    CreatedAt = DateTime.UtcNow,
                    Images = new List<PostImage>()
                };

                // Upload de imagens se fornecidas
                if (images != null && images.Any())
                {
                    for (int i = 0; i < images.Count; i++)
                    {
                        var imageUrl = await _fileUploadService.UploadImageAsync(images[i], "posts");
                        var postImage = new PostImage
                        {
                            ImageUrl = imageUrl,
                            Order = i,
                            CreatedAt = DateTime.UtcNow
                        };
                        post.Images.Add(postImage);
                    }
                }

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                // Recarregar com relacionamentos
                await _context.Entry(post)
                    .Reference(p => p.Author)
                    .LoadAsync();
                await _context.Entry(post)
                    .Reference(p => p.Club)
                    .LoadAsync();

                return post;
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException && ex is not BusinessException)
            {
                _logger.LogError(ex, "Erro ao criar post");
                throw new DatabaseException("Erro ao criar post", ex);
            }
        }

        public async Task<PostDetailDto?> GetByIdAsync(int postId, int currentUserId)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Club)
                .Include(p => p.Images)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null) return null;

            // Verificar permissões do usuário
            var isSystemAdmin = await IsSystemAdminAsync(currentUserId);
            var isClubOwner = post.Club.OwnerId == currentUserId;
            var isClubModerator = await _context.ClubMembers
                .AnyAsync(cm => cm.ClubId == post.ClubId && cm.UserId == currentUserId && cm.IsModerator);

            return post.ToDetailDto(currentUserId, isSystemAdmin, isClubOwner, isClubModerator);
        }

        public async Task<List<PostDto>> GetClubPostsAsync(int clubId, int currentUserId, int skip = 0, int take = 20)
        {
            var posts = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Club)
                .Include(p => p.Images)
                .Include(p => p.Comments)
                .Where(p => p.ClubId == clubId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            // Verificar permissões do usuário uma vez
            var isSystemAdmin = await IsSystemAdminAsync(currentUserId);
            var club = await _context.Clubs.FindAsync(clubId);
            var isClubOwner = club != null && club.OwnerId == currentUserId;
            var isClubModerator = await _context.ClubMembers
                .AnyAsync(cm => cm.ClubId == clubId && cm.UserId == currentUserId && cm.IsModerator);

            return posts.Select(p => p.ToDto(currentUserId, isSystemAdmin, isClubOwner, isClubModerator)).ToList();
        }

        public async Task<Post> UpdatePostAsync(int postId, UpdatePostDto dto, int userId, List<IFormFile>? images)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.Author)
                    .Include(p => p.Club)
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == postId);

                if (post == null)
                    throw new NotFoundException($"Post com ID {postId} não encontrado");

                if (post.AuthorId != userId)
                    throw new ForbiddenException("Apenas o autor pode editar o post");

                post.Content = dto.Content;
                post.UpdatedAt = DateTime.UtcNow;

                // Gerenciar imagens
                if (dto.RemoveImage)
                {
                    // Deletar todas as imagens antigas
                    foreach (var oldImage in post.Images.ToList())
                    {
                        await _fileUploadService.DeleteImageAsync(oldImage.ImageUrl);
                        _context.PostImages.Remove(oldImage);
                    }
                    post.Images.Clear();
                }
                else if (images != null && images.Any())
                {
                    // Deletar imagens antigas
                    foreach (var oldImage in post.Images.ToList())
                    {
                        await _fileUploadService.DeleteImageAsync(oldImage.ImageUrl);
                        _context.PostImages.Remove(oldImage);
                    }
                    post.Images.Clear();

                    // Adicionar novas imagens
                    for (int i = 0; i < images.Count; i++)
                    {
                        var imageUrl = await _fileUploadService.UploadImageAsync(images[i], "posts");
                        var postImage = new PostImage
                        {
                            ImageUrl = imageUrl,
                            Order = i,
                            CreatedAt = DateTime.UtcNow,
                            PostId = post.Id
                        };
                        post.Images.Add(postImage);
                    }
                }

                await _context.SaveChangesAsync();
                return post;
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException && ex is not BusinessException)
            {
                _logger.LogError(ex, "Erro ao atualizar post");
                throw new DatabaseException("Erro ao atualizar post", ex);
            }
        }

        public async Task DeletePostAsync(int postId, int userId)
        {
            try
            {
                var post = await _context.Posts
                    .Include(p => p.Images)
                    .Include(p => p.Club)
                    .FirstOrDefaultAsync(p => p.Id == postId);
                    
                if (post == null)
                    throw new NotFoundException($"Post com ID {postId} não encontrado");

                // Verifica permissões: autor, owner/moderator do clube, ou admin do sistema
                var isAuthor = post.AuthorId == userId;
                var isSystemAdmin = await IsSystemAdminAsync(userId);
                var isClubOwner = post.Club.OwnerId == userId;
                var isClubModerator = await _context.ClubMembers
                    .AnyAsync(cm => cm.ClubId == post.ClubId && cm.UserId == userId && cm.IsModerator);

                if (!isAuthor && !isSystemAdmin && !isClubOwner && !isClubModerator)
                    throw new ForbiddenException("Você não tem permissão para deletar este post");

                // Deletar todas as imagens se existirem
                if (post.Images != null && post.Images.Any())
                {
                    foreach (var image in post.Images)
                    {
                        await _fileUploadService.DeleteImageAsync(image.ImageUrl, "posts");
                    }
                }

                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException && ex is not BusinessException)
            {
                _logger.LogError(ex, "Erro ao deletar post");
                throw new DatabaseException("Erro ao deletar post", ex);
            }
        }

        private async Task<bool> IsSystemAdminAsync(int userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name.ToLower() == "admin");
        }
    }
}
