using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses
{
    public class PaymentDetailsResponseDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public DateTime TransactionDate { get; set; }
        public PaymentStatus Status { get; set; }

    }
}
