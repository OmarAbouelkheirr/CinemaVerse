using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.UserFlow.Movie.Flow
{
    public class MovieDetailsDto
    {
        public int MovieId { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public string MovieDescription { get; set; } = string.Empty;
        public TimeSpan MovieDuration { get; set; }
        public DateOnly ReleaseDate { get; set; }
        public MovieAgeRating MovieAgeRating { get; set; }
        public decimal MovieRating { get; set; }
        public string TrailerUrl { get; set; } = string.Empty;
        public string MoviePoster { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public MovieStatus Status { get; set; }

        public List<CastMemberDto> CastMembers { get; set; } = new();
        public List<GenreDto> Genres { get; set; } = new();
        public List<MovieImageDto> Images { get; set; } = new();
        public List<MovieShowTimeDto> ShowTimes { get; set; } = new();
    }

    public class GenreDto
    {
        public int GenreId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
