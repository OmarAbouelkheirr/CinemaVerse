using CinemaVerse.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminMovie.Requests
{
    public class UpdateMovieRequestDto
    {
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Movie name must be between 1 and 200 characters")]
        public string? MovieName { get; set; }
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string? MovieDescription { get; set; }
        [Range(typeof(TimeSpan), "00:30:00", "05:00:00", ErrorMessage = "Duration must be between 30 minutes and 5 hours")]
        public TimeSpan? MovieDuration { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public List<CastMemberRequestDto>? CastMembers { get; set; }
        [Range(0, 10, ErrorMessage = "Movie rating must be between 0 and 10")]
        public decimal? MovieRating { get; set; }

        [EnumDataType(typeof(MovieAgeRating))]
        public MovieAgeRating? MovieAgeRating { get; set; }

        [Url(ErrorMessage = "Trailer URL must be a valid URL")]
        public string? TrailerUrl { get; set; }

        [Url(ErrorMessage = "Movie poster URL must be a valid URL")]
        [StringLength(500, ErrorMessage = "Movie poster URL must not exceed 500 characters")]
        public string? MoviePoster { get; set; }

        public List<int>? GenreIds { get; set; }
        public List<string>? ImageUrls { get; set; }

        [StringLength(100, ErrorMessage = "Language must not exceed 100 characters")]
        public string? Language { get; set; }

        [EnumDataType(typeof(MovieStatus), ErrorMessage = "Invalid movie status")]
        public MovieStatus? Status { get; set; }
    }
}
