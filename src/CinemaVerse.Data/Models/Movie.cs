namespace CinemaVerse.Data.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public string MovieDescription { get; set;} = string.Empty;
        public TimeSpan MovieDuration {  get; set; }
        public List<string> MovieCast{ get; set; } = new List<string>();
        public decimal MovieRating { get; set; } 
        public string MovieAgeRating { get; set; } = string.Empty;
        public DateOnly ReleaseDate { get; set; }
        public string TrailerUrl { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<MovieImage> MovieImages { get; set; } = new List<MovieImage>();
        public ICollection<MovieShowTime> MovieShowTimes { get; set; } = new List<MovieShowTime>();
        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();


    }
}
