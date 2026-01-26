
using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests
{
    public class AdminTicketFilterDto
    {
        public TicketStatus? Status { get; set; }
        public int? BookingId { get; set; }
        public int? ShowtimeId { get; set; }
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? TicketNumber { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
