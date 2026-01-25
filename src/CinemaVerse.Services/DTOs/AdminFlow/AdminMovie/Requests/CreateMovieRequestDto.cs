using CinemaVerse.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminMovie.Requests
{
    public class CreateMovieRequestDto
    {
        [Required(ErrorMessage = "Movie name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Movie name must be between 1 and 200 characters")]
        public string MovieName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
        public string MovieDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Duration is required")]
        [Range(typeof(TimeSpan), "00:30:00", "05:00:00", ErrorMessage = "Duration must be between 30 minutes and 5 hours")]
        public TimeSpan MovieDuration { get; set; }

        [Required(ErrorMessage = "Release date is required")]
        public DateOnly ReleaseDate { get; set; }

        [Required(ErrorMessage = "At least one cast member is required")]
        [MinLength(1, ErrorMessage = "At least one cast member is required")]
        public List<string> MovieCast { get; set; } = new();

        [Required(ErrorMessage = "Age rating is required")]
        [EnumDataType(typeof(MovieAgeRating), ErrorMessage = "Invalid age rating")]
        public MovieAgeRating MovieAgeRating { get; set; }

        public decimal MovieRating { get; set; }

        [Url(ErrorMessage = "Trailer URL must be a valid URL")]
        public string? TrailerUrl { get; set; }

        [Required(ErrorMessage = "At least one genre is required")]
        [MinLength(1, ErrorMessage = "At least one genre is required")]
        public List<int> GenreIds { get; set; } = new();

        [MinLength(1, ErrorMessage = "At least one image is required")]
        public List<string> ImageUrls { get; set; } = new();

        [EnumDataType(typeof(MovieStatus), ErrorMessage = "Invalid movie status")]
        public MovieStatus Status { get; set; } = MovieStatus.Active;
    }
}