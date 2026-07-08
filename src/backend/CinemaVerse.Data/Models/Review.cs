using CinemaVerse.Data.Models.Users;

namespace CinemaVerse.Data.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public decimal Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public User User { get; set; } = null!;
        public Movie Movie { get; set; } = null!;
    }
}
