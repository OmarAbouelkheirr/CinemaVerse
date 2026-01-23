using CinemaVerse.Services.DTOs.Booking.Requests;
using CinemaVerse.Services.DTOs.Booking.Responses;

namespace CinemaVerse.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDetailsDto> CreateBookingAsync(string userId, CreateBookingRequestDto request);
        Task<List<BookingListDto>> GetUserBookingsAsync(string userId);
        Task<BookingDetailsDto> GetUserBookingByIdAsync(string userId, int bookingId);
        Task<bool> CancelUserBookingAsync(string userId, int bookingId);
    }
}
