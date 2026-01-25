using CinemaVerse.Services.DTOs.Email.Flow;

namespace CinemaVerse.Services.DTOs.Email.Requests
{
    public class BookingConfirmationEmailDto : BaseEmailDto
    {
        public int BookingId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public DateTime ShowStartTime { get; set; }
        public DateTime ShowEndTime { get; set; }
        public string HallNumber { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "EGP";
        public List<TicketInfoDto> Tickets { get; set; } = new();
    }
}
