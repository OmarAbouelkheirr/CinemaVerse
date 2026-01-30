using CinemaVerse.Services.DTOs.UserFlow.Payment.Requests;
using CinemaVerse.Services.DTOs.Payment.Requests;
using CinemaVerse.Services.DTOs.Payment.Response;

namespace CinemaVerse.Services.Interfaces.User
{
    public interface IPaymentService
    {
        Task<CreatePaymentIntentResponseDto> CreatePaymentIntent(int userId, CreatePaymentIntentRequestDto createPaymentDto);
        Task<bool> ConfirmPaymentAsync(int userId, ConfirmPaymentRequestDto confirmPaymentDto);
        Task<bool> RefundPaymentAsync(RefundPaymentRequestDto refundPaymentDto);
        Task<bool> RefundPaymentForUserAsync(int userId, RefundPaymentRequestDto refundPaymentDto);
    }
}
