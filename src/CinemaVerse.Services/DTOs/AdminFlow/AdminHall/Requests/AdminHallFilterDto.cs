using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Requests
{
    public class AdminHallFilterDto
    {
        [StringLength(200, ErrorMessage = "Search term must not exceed 200 characters")]
        public string? SearchTerm { get; set; }
        [StringLength(20, ErrorMessage = "Hall number must not exceed 20 characters")]
        public string? HallNumber { get; set; }
        [StringLength(20, ErrorMessage = "Capacity filter must not exceed 20 characters")]
        public string? Capacity { get; set; }
        [StringLength(50, ErrorMessage = "Hall type must not exceed 50 characters")]
        public string? HallType { get; set; }
        [StringLength(50, ErrorMessage = "Hall status must not exceed 50 characters")]
        public string? HallStatus { get; set; }

        [StringLength(50, ErrorMessage = "Sort by must not exceed 50 characters")]
        public string SortBy { get; set; } = "HallStatus";
        [StringLength(10, ErrorMessage = "Sort order must be 'asc' or 'desc'")]
        public string SortOrder { get; set; } = "desc";

        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
        public int Page { get; set; } = 1;
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 20;
    }
}
