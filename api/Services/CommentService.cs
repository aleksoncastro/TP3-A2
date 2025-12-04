using Microsoft.EntityFrameworkCore;
using MediaMatch.Data;
using MediaMatch.Models.TMDB;
using MediaMatch.DTO.Club;
using MediaMatch.Exceptions;
using MediaMatch.Extensions;

namespace MediaMatch.Services
{
    public class CommentService
    {
        private readonly MediaMatchContext _context;
        private readonly ILogger<CommentService> _logger;

        public CommentService(MediaMatchContext context, ILogger<CommentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Comment> CreateCommentAsync(int postId, CreateCommentDto dto, int authorId)
        {
            try
            {
                // Verificar se o post existe
                var post = await _context.Posts
                    .Include(p => p.Club)
                    .FirstOrDefaultAsync(p => p.Id == postId);

                if (post == null)
                    throw new NotFoundException($"Post com ID {postId} não encontrado");

                // Verificar se o usuário é membro do clube
                var isMember = await _context.ClubMembers
                    .AnyAsync(cm => cm.ClubId == post.ClubId && cm.UserId == authorId);

                if (!isMember)
                    throw new ForbiddenException("Apenas membros podem comentar no clube");

                var comment = new Comment
                {
                    Content = dto.Content,
                    PostId = postId,
                    AuthorId = authorId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                // Recarregar com relacionamentos
                await _context.Entry(comment)
                    .Reference(c => c.Author)
                    .LoadAsync();
                await _context.Entry(comment)
                    .Reference(c => c.Post)
                    .LoadAsync();

                return comment;
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException && ex is not BusinessException)
            {
                _logger.LogError(ex, "Erro ao criar comentário");
                throw new DatabaseException("Erro ao criar comentário", ex);
            }
        }

        public async Task<List<CommentDto>> GetPostCommentsAsync(int postId, int currentUserId)
        {
            var comments = await _context.Comments
                .Include(c => c.Author)
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return comments.Select(c => c.ToDto(currentUserId)).ToList();
        }

        public async Task<Comment> UpdateCommentAsync(int commentId, UpdateCommentDto dto, int userId)
        {
            try
            {
                var comment = await _context.Comments
                    .Include(c => c.Author)
                    .FirstOrDefaultAsync(c => c.Id == commentId);

                if (comment == null)
                    throw new NotFoundException($"Comentário com ID {commentId} não encontrado");

                if (comment.AuthorId != userId)
                    throw new ForbiddenException("Apenas o autor pode editar o comentário");

                comment.Content = dto.Content;
                comment.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return comment;
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException && ex is not BusinessException)
            {
                _logger.LogError(ex, "Erro ao atualizar comentário");
                throw new DatabaseException("Erro ao atualizar comentário", ex);
            }
        }

        public async Task DeleteCommentAsync(int commentId, int userId)
        {
            try
            {
                var comment = await _context.Comments
                    .Include(c => c.Post)
                    .ThenInclude(p => p.Club)
                    .FirstOrDefaultAsync(c => c.Id == commentId);
                    
                if (comment == null)
                    throw new NotFoundException($"Comentário com ID {commentId} não encontrado");

                // Verifica permissões: autor, owner/moderator do clube, ou admin do sistema
                var isAuthor = comment.AuthorId == userId;
                var isSystemAdmin = await IsSystemAdminAsync(userId);
                var isClubOwner = comment.Post.Club.OwnerId == userId;
                var isClubModerator = await _context.ClubMembers
                    .AnyAsync(cm => cm.ClubId == comment.Post.ClubId && cm.UserId == userId && cm.IsModerator);

                if (!isAuthor && !isSystemAdmin && !isClubOwner && !isClubModerator)
                    throw new ForbiddenException("Você não tem permissão para deletar este comentário");

                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) when (ex is not NotFoundException && ex is not ForbiddenException && ex is not BusinessException)
            {
                _logger.LogError(ex, "Erro ao deletar comentário");
                throw new DatabaseException("Erro ao deletar comentário", ex);
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
