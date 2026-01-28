using CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Responses;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.Ticket.Response;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminUserService
    {
        Task<int> CreateUserAsync(CreateUserRequestDto request);
        Task<int> UpdateUserAsync(int userId, UpdateUserRequestDto request);
        Task DeleteUserAsync(int userId);

        Task<UserDetailsDto?> GetUserByIdAsync(int userId);
        Task<UserDetailsDto?> GetUserByEmailAsync(string email);
        Task<PagedResultDto<UserDetailsDto>> GetAllUsersAsync(AdminUserFilterDto filter);

        Task<bool> ActivateUserAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ConfirmUserEmailAsync(int userId);
        Task<bool> UnconfirmUserEmailAsync(int userId);

        Task<PagedResultDto<BookingDetailsDto>> GetUserBookingsAsync(int userId, AdminBookingFilterDto filter);
        Task<PagedResultDto<TicketDetailsDto>> GetUserTicketsAsync(int userId, AdminTicketFilterDto filter);
        Task<PagedResultDto<PaymentDetailsResponseDto>> GetUserPaymentsAsync(int userId, AdminPaymentFilterDto filter);
    }
}
