using CinemaVerse.Services.DTOs.Email.Flow;

namespace CinemaVerse.Services.DTOs.Email.Requests
{
    public class PaymentConfirmationEmailDto : BaseEmailDto
    {
        public int BookingId { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public DateTime TransactionDate { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public DateTime ShowStartTime { get; set; }
    }
}
