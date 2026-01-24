using CinemaVerse.Services.DTOs.Booking.Requests;
using CinemaVerse.Services.DTOs.Booking.Responses;
using CinemaVerse.Services.DTOs.Ticket.Response;

namespace CinemaVerse.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDetailsDto> CreateBookingAsync(int userId, CreateBookingRequestDto request);
        Task<BookingDetailsDto> ConfirmBookingAsync(int userId, int bookingId);
        Task<List<BookingListDto>> GetUserBookingsAsync(int userId);
        Task<BookingDetailsDto> GetUserBookingByIdAsync(int userId, int bookingId);
        Task<bool> CancelUserBookingAsync(int userId, int bookingId);
    }
}
