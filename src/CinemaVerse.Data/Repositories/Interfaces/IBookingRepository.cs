using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking?> GetBookingWithDetailsAsync(int BookingId);
        Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus Status);
        Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid UserId);
    }
}
