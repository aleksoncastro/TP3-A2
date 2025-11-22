using MediaMatch.Data;
using MediaMatch.Models.TMDB;
using Microsoft.EntityFrameworkCore;

namespace MediaMatch.Services
{
    public class MediaListService
    {
        private readonly MediaMatchContext _context;

        public MediaListService(MediaMatchContext context)
        {
            _context = context;
        }

        public async Task<MediaList> CreateListAsync(MediaList list)
        {
            _context.MediaLists.Add(list);
            await _context.SaveChangesAsync();
            return list;
        }

        public async Task<List<MediaList>> GetListsByClubAsync(int clubId)
        {
            return await _context.MediaLists
                .Where(l => l.ClubId == clubId)
                .Include(l => l.Items)
                .ThenInclude(i => i.MediaItem)
                .ToListAsync();
        }

        public async Task<List<MediaList>> GetListsByUserAsync(int userId)
        {
            return await _context.MediaLists
                .Where(l => l.UserId == userId)
                .Include(l => l.Items)
                .ThenInclude(i => i.MediaItem)
                .ToListAsync();
        }
    }
}
