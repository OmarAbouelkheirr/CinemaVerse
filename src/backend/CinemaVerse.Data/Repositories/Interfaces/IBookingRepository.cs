using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;

namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking?> GetBookingWithDetailsAsync(int BookingId);
        Task<bool> UpdateBookingStatusAsync(int BookingId, BookingStatus NewStatus);
        Task<IEnumerable<Booking>> GetConfirmedBookingsInShowTimeWindowAsync(DateTime windowStartUtc, DateTime windowEndUtc);
    }
}
