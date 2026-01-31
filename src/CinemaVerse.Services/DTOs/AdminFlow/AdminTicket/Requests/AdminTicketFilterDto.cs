using System.ComponentModel.DataAnnotations;
using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests
{
    public class AdminTicketFilterDto
    {
        public TicketStatus? Status { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Booking ID must be greater than 0")]
        public int? BookingId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Showtime ID must be greater than 0")]
        public int? ShowtimeId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be greater than 0")]
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [StringLength(50, ErrorMessage = "Ticket number must not exceed 50 characters")]
        public string? TicketNumber { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
        public int Page { get; set; } = 1;
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;
    }
}
