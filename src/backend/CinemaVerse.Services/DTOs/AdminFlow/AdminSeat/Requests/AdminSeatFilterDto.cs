using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Requests
{
    public class AdminSeatFilterDto
    {
        [StringLength(50, ErrorMessage = "Search term must not exceed 50 characters")]
        public string? SearchTerm { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Hall ID must be greater than 0")]
        public int? HallId { get; set; }

        [StringLength(50, ErrorMessage = "Sort by must not exceed 50 characters")]
        public string SortBy { get; set; } = "SeatLabel";
        [StringLength(10, ErrorMessage = "Sort order must be 'asc' or 'desc'")]
        public string SortOrder { get; set; } = "asc";

        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
        public int Page { get; set; } = 1;
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 20;
    }
}
