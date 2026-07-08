using System.ComponentModel.DataAnnotations;
using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests
{
    public class AdminBookingFilterDto
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
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be greater than 0")]
        public int? UserId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Movie showtime ID must be greater than 0")]
        public int? MovieShowTimeId { get; set; }
        public BookingStatus? Status { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Min amount cannot be negative")]
        public decimal? MinAmount { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Max amount cannot be negative")]
        public decimal? MaxAmount { get; set; }
    }
}
