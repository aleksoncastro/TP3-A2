using MediaMatch.DTO.Club;
using MediaMatch.Models.TMDB;

namespace MediaMatch.Extensions
{
    public static class ClubExtensions
    {
        public static ClubDto ToDto(this Club club, int? currentUserId = null)
        {
            return new ClubDto
            {
                Id = club.Id,
                Name = club.Name,
                Description = club.Description,
                ImageUrl = club.ImageUrl,
                CreatedAt = club.CreatedAt,
                OwnerId = club.OwnerId,
                OwnerName = club.Owner?.UserName,
                MembersCount = club.Members?.Count ?? 0,
                MediaListsCount = club.MediaLists?.Count ?? 0,
                IsOwner = currentUserId.HasValue && club.OwnerId == currentUserId.Value,
                IsMember = currentUserId.HasValue && (club.Members?.Any(m => m.UserId == currentUserId.Value) ?? false)
            };
        }

        public static ClubDetailDto ToDetailDto(this Club club, int? currentUserId = null)
        {
            return new ClubDetailDto
            {
                Id = club.Id,
                Name = club.Name,
                Description = club.Description,
                ImageUrl = club.ImageUrl,
                CreatedAt = club.CreatedAt,
                OwnerId = club.OwnerId,
                OwnerName = club.Owner?.UserName,
                MembersCount = club.Members?.Count ?? 0,
                MediaListsCount = club.MediaLists?.Count ?? 0,
                IsOwner = currentUserId.HasValue && club.OwnerId == currentUserId.Value,
                IsMember = currentUserId.HasValue && (club.Members?.Any(m => m.UserId == currentUserId.Value) ?? false),
                Members = club.Members?.Select(m => new ClubMemberDto
                {
                    UserId = m.UserId,
                    UserName = m.User?.UserName,
                    Email = m.User?.Email,
                    JoinedAt = m.JoinedAt,
                    IsModerator = m.IsModerator
                }).ToList() ?? new List<ClubMemberDto>(),
                MediaLists = club.MediaLists?.Select(ml => new MediaListSummaryDto
                {
                    Id = ml.Id,
                    Name = ml.Name,
                    Description = ml.Description,
                    IsPublic = ml.IsPublic,
                    CreatedAt = ml.CreatedAt,
                    ItemsCount = ml.Items?.Count ?? 0
                }).ToList() ?? new List<MediaListSummaryDto>()
            };
        }
    }
}
