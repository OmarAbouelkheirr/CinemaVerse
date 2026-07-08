using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.Payment.Requests
{
    public class ConfirmPaymentRequestDto
    {
        [Required(ErrorMessage = "Payment intent ID is required")]
        [MinLength(1, ErrorMessage = "Payment intent ID cannot be empty")]
        public string PaymentIntentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Booking ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Booking ID must be greater than 0")]
        public int BookingId { get; set; }
    }
}
