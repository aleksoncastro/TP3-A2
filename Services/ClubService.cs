using MediaMatch.Data;
using MediaMatch.Models.TMDB;
using Microsoft.EntityFrameworkCore;

namespace MediaMatch.Services
{
    public class ClubService
    {
        private readonly MediaMatchContext _context;

        public ClubService(MediaMatchContext context)
        {
            _context = context;
        }

        public async Task<Club> CreateClubAsync(Club club)
        {
            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();
            return club;
        }

        public async Task<List<Club>> GetAllAsync()
        {
            return await _context.Clubs
                .Include(c => c.Members)
                .Include(c => c.MediaLists)
                .ToListAsync();
        }

        public async Task<Club?> GetByIdAsync(int id)
        {
            return await _context.Clubs
                .Include(c => c.Members)
                .Include(c => c.MediaLists)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> AddMemberAsync(int clubId, int userId)
        {
            var exists = await _context.ClubMembers
                .AnyAsync(m => m.ClubId == clubId && m.UserId == userId);

            if (exists) return false;

            _context.ClubMembers.Add(new ClubMember
            {
                ClubId = clubId,
                UserId = userId
            });

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
