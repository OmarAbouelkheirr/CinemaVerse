using CinemaVerse.Data.Enums;

namespace CinemaVerse.Data.Models
{
    public class MovieCastMember
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public CastRoleType RoleType { get; set; }
        public string? CharacterName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsLead { get; set; }

        public Movie Movie { get; set; } = null!;
    }
}
