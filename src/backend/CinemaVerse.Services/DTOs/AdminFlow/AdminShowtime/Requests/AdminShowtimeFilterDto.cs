using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminShowtime.Requests
{
    public class AdminShowtimeFilterDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
        public int Page { get; set; } = 1;
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 20;

        [StringLength(50, ErrorMessage = "Sort by must not exceed 50 characters")]
        public string? SortBy { get; set; }
        [StringLength(10, ErrorMessage = "Sort order must be 'asc' or 'desc'")]
        public string? SortOrder { get; set; }

        [StringLength(200, ErrorMessage = "Search term must not exceed 200 characters")]
        public string? SearchTerm { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Movie ID must be greater than 0")]
        public int? MovieId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Hall ID must be greater than 0")]
        public int? HallId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Branch ID must be greater than 0")]
        public int? BranchId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Price min cannot be negative")]
        public decimal? PriceMin { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Price max cannot be negative")]
        public decimal? PriceMax { get; set; }
    }
}
