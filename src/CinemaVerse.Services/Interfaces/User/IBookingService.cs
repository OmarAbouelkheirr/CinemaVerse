using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.User
{
    public interface IBookingService
    {
        Task<BookingDetailsDto> CreateBookingAsync(CreateBookingRequestDto request);
        Task<BookingDetailsDto> ConfirmBookingAsync(int userId, int bookingId);
        Task<List<BookingListDto>> GetUserBookingsAsync(int userId);
        Task<BookingDetailsDto> GetUserBookingByIdAsync(int userId, int bookingId);
        Task<bool> CancelUserBookingAsync(int userId, int bookingId);
    }
}
