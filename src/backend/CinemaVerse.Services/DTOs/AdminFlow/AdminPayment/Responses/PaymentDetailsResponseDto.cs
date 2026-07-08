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
        public string PaymentIntentId { get; set; } = string.Empty;

        // Enriched user context
        public string UserEmail { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;

        // Enriched movie context
        public string MovieName { get; set; } = string.Empty;
    }
}
