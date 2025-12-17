using CinemaVerse.Services.DTOs.Booking.Requests;
using CinemaVerse.Services.DTOs.Booking.Responses;

namespace CinemaVerse.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDetailsDto> CreateBookingAsync(int UserId, CreateBookingRequestDto request);
        Task<List<BookingListDto>> GetUserBookingsAsync(int UserId);
        Task<BookingDetailsDto> GetUserBookingByIdAsync(int UserId, int BookingId);
        Task <bool> CancelUserBookingAsync(int UserId, int BookingId);
    }
}
