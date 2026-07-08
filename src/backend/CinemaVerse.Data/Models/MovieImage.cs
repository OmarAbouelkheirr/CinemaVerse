namespace CinemaVerse.Data.Models
{
    public class MovieImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int MovieId { get; set; }

        // Navigation Properties
        public Movie Movie { get; set; } = null!;
    }
}
