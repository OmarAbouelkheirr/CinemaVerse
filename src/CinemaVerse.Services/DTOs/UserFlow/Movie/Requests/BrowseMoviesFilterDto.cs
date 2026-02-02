using System.ComponentModel.DataAnnotations;
using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.Movie.Requests
{
    public class BrowseMoviesFilterDto
    {
        [StringLength(200, ErrorMessage = "Search term must not exceed 200 characters")]
        public string? SearchTerm { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Genre ID must be greater than 0")]
        public int? GenreId { get; set; }
        public MovieAgeRating? AgeRating { get; set; }
        public DateOnly? ReleaseDateFrom { get; set; }
        public DateOnly? ReleaseDateTo { get; set; }
        public MovieStatus? Status { get; set; }
        [StringLength(100, ErrorMessage = "Language must not exceed 100 characters")]
        public string? Language { get; set; }

        [StringLength(50, ErrorMessage = "Sort by must not exceed 50 characters")]
        public string SortBy { get; set; } = "ReleaseDate";
        [StringLength(10, ErrorMessage = "Sort order must be 'asc' or 'desc'")]
        public string SortOrder { get; set; } = "desc";

        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
        public int Page { get; set; } = 1;
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 20;
    }
}
