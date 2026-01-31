using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses;

namespace CinemaVerse.Services.Mappers
{
    public static class PaymentMapper
    {
        public static PaymentDetailsResponseDto ToPaymentDetailsResponseDto(BookingPayment payment)
        {
            return new PaymentDetailsResponseDto
            {
                PaymentId = payment.Id,
                BookingId = payment.BookingId,
                Amount = payment.Amount,
                Currency = payment.Currency ?? "EGP",
                TransactionDate = payment.TransactionDate,
                Status = payment.Status
            };
        }
    }
}
