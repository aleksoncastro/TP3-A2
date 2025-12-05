using System.ComponentModel.DataAnnotations;

namespace MediaMatch.DTO.Club
{
    public class CreateClubDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(150, ErrorMessage = "Nome não pode ter mais de 150 caracteres")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Descrição não pode ter mais de 500 caracteres")]
        public string? Description { get; set; }
    }

    public class UpdateClubDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(150, ErrorMessage = "Nome não pode ter mais de 150 caracteres")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Descrição não pode ter mais de 500 caracteres")]
        public string? Description { get; set; }
        
        // Flag para indicar se deve remover a imagem atual
        public bool RemoveImage { get; set; } = false;
    }

    public class ClubDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public int MembersCount { get; set; }
        public int MediaListsCount { get; set; }
        public bool IsOwner { get; set; }
        public bool IsMember { get; set; }
    }

    public class ClubDetailDto : ClubDto
    {
        public List<ClubMemberDto> Members { get; set; } = new();
        public List<MediaListSummaryDto> MediaLists { get; set; } = new();
    }

    public class ClubMemberDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsModerator { get; set; }
    }

    public class MediaListSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ItemsCount { get; set; }
    }

    public class ClubFilterDto
    {
        public string? SearchTerm { get; set; }
        public int? OwnerId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "newest"; // newest, oldest, members, name
        public string? SortOrder { get; set; } = "desc"; // asc, desc
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
    }

    public class AddMemberDto
    {
        [Required]
        public int UserId { get; set; }
    }
}
