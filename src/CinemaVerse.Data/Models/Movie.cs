using CinemaVerse.Data.Enums;

namespace CinemaVerse.Data.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public string MovieDescription { get; set;} = string.Empty;
        public TimeSpan MovieDuration {  get; set; }
        public decimal MovieRating { get; set; } 
        public MovieAgeRating MovieAgeRating { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public string TrailerUrl { get; set; } = string.Empty;
        public string MoviePoster { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public MovieStatus Status { get; set; } = MovieStatus.Active;

        // Navigation Properties
        public ICollection<MovieCastMember> CastMembers { get; set; } = new List<MovieCastMember>();
        public ICollection<MovieImage> MovieImages { get; set; } = new List<MovieImage>();
        public ICollection<MovieShowTime> MovieShowTimes { get; set; } = new List<MovieShowTime>();
        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
