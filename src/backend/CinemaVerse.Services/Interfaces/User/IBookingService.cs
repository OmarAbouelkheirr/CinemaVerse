using CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.User
{
    public interface IBookingService
    {
        Task<BookingDetailsDto> CreateBookingAsync(CreateBookingRequestDto request);
        Task<BookingDetailsDto> ConfirmBookingAsync(int userId, int bookingId);
        Task<PagedResultDto<BookingDetailsDto>> GetUserBookingsAsync(int userId, AdminBookingFilterDto filter);
        Task<BookingDetailsDto> GetUserBookingByIdAsync(int userId, int bookingId);
        Task<bool> CancelUserBookingAsync(int userId, int bookingId);
    }
}
