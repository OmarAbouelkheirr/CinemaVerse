using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.UserFlow.Movie.Flow
{
    public class CastMemberDto
    {
        public int Id { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public CastRoleType RoleType { get; set; }
        public string? CharacterName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsLead { get; set; }
    }
}
