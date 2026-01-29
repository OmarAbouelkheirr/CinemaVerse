using CinemaVerse.Services.DTOs.UserFlow.Payment.Requests;
using CinemaVerse.Services.DTOs.Payment.Requests;
using CinemaVerse.Services.DTOs.Payment.Response;

namespace CinemaVerse.Services.Interfaces.User
{
    public interface IPaymentService
    {
        Task<CreatePaymentIntentResponseDto> CreatePaymentIntent(int userId, CreatePaymentIntentRequestDto CreatePaymentDto);
        Task<bool> ConfirmPaymentAsync(int userId, ConfirmPaymentRequestDto ConfrimPaymentDto);
        Task<bool> RefundPaymentAsync(RefundPaymentRequestDto RefundPaymentDto);
        Task<bool> RefundPaymentForUserAsync(int userId, RefundPaymentRequestDto refundPaymentDto);
    }
}
