using CinemaVerse.Data.Data;
using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Data.Repositories.Implementations
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext Context, ILogger<Booking> Logger) : base(Context, Logger)
        {
        }

        public async Task<bool> UpdateBookingStatusAsync(int BookingId, BookingStatus NewStatus)
        {
            try
            {
                _logger.LogInformation("Updating booking {BookingId} status to {NewStatus}", BookingId, NewStatus);

                if (BookingId <= 0)
                {
                    _logger.LogWarning("Invalid BookingId: {BookingId}", BookingId);
                    throw new ArgumentException("BookingId must be greater than zero.", nameof(BookingId));
                }

                var booking = await _dbSet.FirstOrDefaultAsync(b => b.Id == BookingId);

                if (booking == null)
                {
                    _logger.LogWarning("Booking with Id {BookingId} not found", BookingId);
                    return false;
                }

                if (booking.Status == NewStatus)
                {
                    _logger.LogInformation("Booking {BookingId} already has status {NewStatus}, no update needed", BookingId, NewStatus);
                    return true;
                }

                booking.Status = NewStatus;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated booking {BookingId} status to {NewStatus}", BookingId, NewStatus);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Updating booking {BookingId} status", BookingId);
                throw;
            }
        }

        public async Task<Booking?> GetBookingWithDetailsAsync(int BookingId)
        {
            try
            {
                _logger.LogInformation("Getting bookings {BookingId} with details", BookingId);
                var Result = await _dbSet
                    .Include(b => b.User)
                    .Include(b => b.Tickets)
                        .ThenInclude(t => t.Seat)
                    .Include(b => b.MovieShowTime)
                        .ThenInclude(mst => mst.Movie)
                            .ThenInclude(m => m.MovieImages)
                    .Include(b => b.MovieShowTime)
                        .ThenInclude(mst => mst.Hall)
                            .ThenInclude(h => h.Branch)
                    .Include(b => b.BookingPayments)
                    .Include(b => b.BookingSeats)
                        .ThenInclude(bs => bs.Seat)
                    .FirstOrDefaultAsync(b => b.Id == BookingId);

                if (Result == null)
                {
                    _logger.LogWarning("Booking with Id {BookingId} not found", BookingId);
                }
                else
                {
                    _logger.LogDebug("Successfully retrieved booking {BookingId} with all details", BookingId);
                }
                return Result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId} with details", BookingId);
                throw;
            }
        }

        public async Task<IEnumerable<Booking>> GetConfirmedBookingsInShowTimeWindowAsync(DateTime windowStartUtc, DateTime windowEndUtc)
        {
            try
            {
                _logger.LogDebug("Getting confirmed bookings with show time in window {Start}–{End}", windowStartUtc, windowEndUtc);
                var result = await _dbSet
                    .AsNoTracking()
                    .Where(b => b.Status == BookingStatus.Confirmed
                        && b.MovieShowTime != null
                        && b.MovieShowTime.ShowStartTime >= windowStartUtc
                        && b.MovieShowTime.ShowStartTime <= windowEndUtc)
                    .Include(b => b.User)
                    .Include(b => b.Tickets)
                        .ThenInclude(t => t.Seat)
                    .Include(b => b.MovieShowTime)
                        .ThenInclude(mst => mst.Movie)
                    .Include(b => b.MovieShowTime)
                        .ThenInclude(mst => mst.Hall)
                            .ThenInclude(h => h.Branch)
                    .Include(b => b.BookingSeats)
                        .ThenInclude(bs => bs.Seat)
                    .OrderBy(b => b.MovieShowTime!.ShowStartTime)
                    .ToListAsync();
                _logger.LogDebug("Retrieved {Count} confirmed bookings in show time window", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting confirmed bookings in show time window");
                throw;
            }
        }
    }
}
