using CinemaVerse.Data.Enums;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminBookingService
    {
        public Task<int> CreateBookingAsync(CreateBookingRequestDto request);
        public Task<int> UpdateBookingStatusAsync(int bookingId, BookingStatus newStatus);
        public Task DeleteBookingAsync(int bookingId);
        public Task<BookingDetailsDto?> GetBookingByIdAsync(int bookingId);
        public Task<PagedResultDto<BookingDetailsDto>> GetAllBookingsAsync(AdminBookingFilterDto filter);
    }
}
