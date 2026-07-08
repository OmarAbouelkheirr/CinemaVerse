using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.UserFlow.Payment.Requests
{
    public class CreatePaymentIntentRequestDto
    {
        [Required(ErrorMessage = "Booking ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Booking ID must be greater than 0")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter code (e.g. EGP, USD)")]
        public string Currency { get; set; } = "EGP";

        [StringLength(50, ErrorMessage = "Payment method type must not exceed 50 characters")]
        public string PaymentMethodType { get; set; } = string.Empty;
    }
}
