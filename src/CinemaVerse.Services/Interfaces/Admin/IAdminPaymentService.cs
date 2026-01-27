using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminPaymentService
    {
        public Task<PagedResultDto<PaymentDetailsResponseDto>> GetAllPaymentsAsync(AdminPaymentFilterDto filter);
        public Task<PaymentDetailsResponseDto?> GetPaymentByIdAsync(int paymentId);

    }
}
