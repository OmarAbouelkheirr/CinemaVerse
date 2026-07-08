namespace CinemaVerse.Services.DTOs.UserFlow.Movie.Flow
{
    public class MovieCardDto
    {
        public int MovieId { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public string MovieDuration { get; set; } = string.Empty;
        public string? MoviePosterImageUrl { get; set; }
        public List<string> Genres { get; set; } = new();
        public string? Language { get; set; }
        public decimal MovieRating { get; set; }

    }
}
