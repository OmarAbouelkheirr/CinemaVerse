using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminPaymentService
    {
        Task<PagedResultDto<PaymentDetailsResponseDto>> GetAllPaymentsAsync(AdminPaymentFilterDto filter);
        Task<PaymentDetailsResponseDto> GetPaymentByIdAsync(int paymentId);
        Task<PaymentSummaryDto> GetPaymentSummaryAsync();
    }
}
