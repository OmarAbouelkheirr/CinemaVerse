using CinemaVerse.Services.DTOs.Email.Flow;

namespace CinemaVerse.Services.DTOs.Email.Requests
{
    public class BookingCancellationEmailDto : BaseEmailDto
    {
        public int BookingId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public DateTime ShowStartTime { get; set; }
        public decimal RefundAmount { get; set; }
        public string Currency { get; set; } = "EGP";
        public string CancellationReason { get; set; } = string.Empty;
    }
}
