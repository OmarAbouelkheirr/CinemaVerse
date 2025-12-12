using CinemaVerse.Data.Data;
using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Data.Repositories.Implementations
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext Context, ILogger<Booking> Logger) : base(Context, Logger)
        {
        }

        public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus Status)
        {
            try
            {
                _logger.LogInformation("Getting bookings by status {Status}", Status);
                var Result = await _dbSet
                    .AsNoTracking()
                    .Where(b => b.Status == Status)
                    .Include(b=>b.User)
                    .Include(b=>b.Tickets)
                    .Include(b => b.MovieShowTime)
                        .ThenInclude(m => m.Movie)
                    .Include(b=>b.BookingPayments)
                    .OrderByDescending(b=>b.CreatedAt)
                    .ToListAsync();

                _logger.LogDebug("Retrieved {Count} bookings with status {Status}", Result.Count, Status);
                return Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings by status {Status}", Status);
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
                    .Include(b => b.MovieShowTime)
                        .ThenInclude(m => m.Movie)
                    .Include(b => b.BookingPayments)
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

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(Guid UserId)
        {
            try
            {
                _logger.LogInformation("Getting bookings for user: {UserId}", UserId);

                var Result = await _dbSet
                    .AsNoTracking()
                    .Where(b => b.UserId == UserId)
                    .Include(b => b.User)
                    .Include(b => b.Tickets)
                    .Include(b => b.MovieShowTime)
                        .ThenInclude(m => m.Movie)
                    .Include(b => b.BookingPayments)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToListAsync();

                _logger.LogDebug("Retrieved {Count} bookings for user {UserId}", Result.Count, UserId);
                return Result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user {UserId}", UserId);
                throw;
            }
        }
    }
}
