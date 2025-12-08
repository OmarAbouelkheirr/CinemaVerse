namespace CinemaVerse.Data.Models
{
    public class Genre
    {
        public int Id { get; set; }
        public string GenreName { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();

    }
}
