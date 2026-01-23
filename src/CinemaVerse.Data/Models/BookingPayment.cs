using CinemaVerse.Data.Enums;

namespace CinemaVerse.Data.Models
{
    public class BookingPayment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string Currency { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }

        // Navigation Properties
        public Booking Booking { get; set; } = null!;
    }
}
