using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminMovie.Requests
{
    public class AdminMovieFilterDto
    {
        // Search & Filters
        public string? SearchTerm { get; set; }
        public int? GenreId { get; set; }
        public MovieAgeRating? AgeRating { get; set; }
        public DateOnly? ReleaseDateFrom { get; set; }
        public DateOnly? ReleaseDateTo { get; set; }
        public MovieStatus? Status { get; set; } // Draft, Active, Archived

        // Sorting
        public string SortBy { get; set; } = "ReleaseDate"; // MovieName, ReleaseDate, Rating, CreatedAt
        public string SortOrder { get; set; } = "desc"; // asc, desc

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}