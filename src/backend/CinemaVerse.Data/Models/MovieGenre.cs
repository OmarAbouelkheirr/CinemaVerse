namespace CinemaVerse.Data.Models
{
    public class MovieGenre
    {
        public int GenreID { get; set; }
        public int MovieID { get; set; }

        // Navigation Properties
        public Movie Movie { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
    }
}
