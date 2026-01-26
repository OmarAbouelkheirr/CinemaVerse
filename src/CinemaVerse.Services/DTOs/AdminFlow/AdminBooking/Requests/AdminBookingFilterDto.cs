using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests
{
    public class AdminBookingFilterDto
    {
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; } // createdat, totalamount, status, expiresat
        public string? SortOrder { get; set; } // asc, desc

        // Filters
        public string? SearchTerm { get; set; } // Search by user email or movie name
        public int? UserId { get; set; }
        public int? MovieShowTimeId { get; set; }
        public BookingStatus? Status { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}
