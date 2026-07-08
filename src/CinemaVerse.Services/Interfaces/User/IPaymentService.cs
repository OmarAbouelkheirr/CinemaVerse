using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.Payment.Requests;
using CinemaVerse.Services.DTOs.Payment.Response;
using CinemaVerse.Services.DTOs.UserFlow.Payment.Requests;

namespace CinemaVerse.Services.Interfaces.User
{
    public interface IPaymentService
    {
        Task<CreatePaymentIntentResponseDto> CreatePaymentIntent(int userId, CreatePaymentIntentRequestDto createPaymentDto);
        Task<bool> ConfirmPaymentAsync(int userId, ConfirmPaymentRequestDto confirmPaymentDto);
        Task<bool> RefundPaymentAsync(RefundPaymentRequestDto refundPaymentDto);
        Task<bool> RefundPaymentForUserAsync(int userId, RefundPaymentRequestDto refundPaymentDto);
        Task<PagedResultDto<PaymentDetailsResponseDto>> GetUserPaymentsAsync(int userId, AdminPaymentFilterDto filter);
    }
}
