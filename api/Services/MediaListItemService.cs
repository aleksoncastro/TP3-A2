using MediaMatch.Data;
using MediaMatch.Models.TMDB;
using Microsoft.EntityFrameworkCore;

namespace MediaMatch.Services
{
    public class MediaListItemService
    {
        private readonly MediaMatchContext _context;

        public MediaListItemService(MediaMatchContext context)
        {
            _context = context;
        }

        public async Task<MediaListItem> AddItemAsync(MediaListItem item)
        {
            _context.MediaListItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }
    }
}
