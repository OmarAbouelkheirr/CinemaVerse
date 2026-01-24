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
    public class MovieShowTimeRepository : Repository<MovieShowTime>, IMovieShowTimeRepository
    {
        public MovieShowTimeRepository(AppDbContext Context, ILogger<MovieShowTime> Logger) : base(Context, Logger)
        {
        }

        public async Task<IEnumerable<Seat>> GetAvailableSeatsAsync(int movieShowTimeId)
        {
            try
            {
                _logger.LogInformation("Getting available seats for movie show time {MovieShowTimeId}", movieShowTimeId);

                // 1. نجيب الـ MovieShowTime مع الـ Hall والمقاعد
                var movieShowTime = await _dbSet
                    .AsNoTracking()
                    .Include(mst => mst.Hall)
                        .ThenInclude(h => h.Seats)
                    .FirstOrDefaultAsync(mst => mst.Id == movieShowTimeId);

                if (movieShowTime == null)
                {
                    _logger.LogWarning("Movie show time with ID {MovieShowTimeId} not found", movieShowTimeId);
                    return Enumerable.Empty<Seat>();
                }

                // 2. نجيب IDs المقاعد المحجوزة (Reserved Seats)
                var reservedSeatIds = await _context.Tickets
                    .AsNoTracking()
                    .Where(t => t.Booking.MovieShowTimeId == movieShowTimeId)
                    .Select(t => t.SeatId)
                    .ToListAsync();

                // 3. نجيب المقاعد المتاحة (كل المقاعد - المقاعد المحجوزة)
                var availableSeats = movieShowTime.Hall.Seats
                    .Where(seat => !reservedSeatIds.Contains(seat.Id))
                    .OrderBy(seat => seat.SeatLabel)
                    .ToList();

                _logger.LogInformation("Retrieved {AvailableCount} available seats out of {TotalCount} total seats for movie show time {MovieShowTimeId}",
                    availableSeats.Count, movieShowTime.Hall.Seats.Count, movieShowTimeId);

                return availableSeats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available seats for movie show time {MovieShowTimeId}", movieShowTimeId);
                throw;
            }
        }
        public async Task<List<Seat>> GetReservedSeatsAsync(int movieShowTimeId)
        {
            var reservedSeats = await (
                from t in _context.Tickets.AsNoTracking()
                where t.Booking.MovieShowTimeId == movieShowTimeId
                      && t.Status != TicketStatus.Cancelled
                      && t.Booking.Status != BookingStatus.Cancelled
                join s in _context.Seats.AsNoTracking() on t.SeatId equals s.Id
                select s
            )
            .Distinct()
            .OrderBy(s => s.SeatLabel)
            .ToListAsync();

            return reservedSeats;
        }

        public async Task<MovieShowTime?> GetMovieShowTimeWithDetailsAsync(int movieShowTimeId)
        {
            try
            {
                _logger.LogInformation("Getting movie show time {MovieShowTimeId} with details", movieShowTimeId);

                var result = await _dbSet
                    .AsNoTracking()
                    .Include(mst => mst.Movie)
                        .ThenInclude(m => m.MovieGenres)
                    .Include(mst => mst.Movie)
                        .ThenInclude(m => m.MovieImages)
                    .Include(mst => mst.Hall)
                        .ThenInclude(h => h.Branch)
                    .Include(mst => mst.Bookings)
                        .ThenInclude(b => b.User)
                    .Include(mst => mst.Bookings)
                        .ThenInclude(b => b.Tickets)
                            .ThenInclude(t => t.Seat)
                    .FirstOrDefaultAsync(mst => mst.Id == movieShowTimeId);

                if (result == null)
                {
                    _logger.LogWarning("Movie show time with ID {MovieShowTimeId} not found", movieShowTimeId);
                }
                else
                {
                    _logger.LogInformation("Successfully retrieved movie show time {MovieShowTimeId} with details. " +
                        "Found {BookingsCount} bookings", movieShowTimeId, result.Bookings.Count);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie show time {MovieShowTimeId} with details", movieShowTimeId);
                throw;
            }
        }

        public async Task<IEnumerable<Ticket>> GetReservedTicketsAsync(int movieShowTimeId)
        {
            try
            {
                _logger.LogInformation("Getting Reserved Tickets for movie show time {MovieShowTimeId}", movieShowTimeId);

                var tickets = await _context.Tickets
                    .AsNoTracking()
                    .Where(t => t.Booking.MovieShowTimeId == movieShowTimeId)
                    .Include(t => t.Booking)
                        .ThenInclude(b => b.User)
                    .Include(t => t.Seat)
                    .OrderBy(t => t.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} reserved tickets for movie show time {MovieShowTimeId}", tickets.Count, movieShowTimeId);

                return tickets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Reserved Tickets for movie show time {MovieShowTimeId}", movieShowTimeId);
                throw;
            }
        }
        public async Task<bool> IsSeatReservedAsync(int movieShowTimeId, int seatId)
        {
            try
            {
                _logger.LogInformation("Checking if seat {SeatId} is reserved for movie show time {MovieShowTimeId}",
                    seatId, movieShowTimeId);

                var isReserved = await _context.Tickets
                    .AsNoTracking()
                    .AnyAsync(t => t.Booking.MovieShowTimeId == movieShowTimeId
                                && t.SeatId == seatId
                                && t.Status != TicketStatus.Cancelled
                                && t.Booking.Status != BookingStatus.Cancelled);

                _logger.LogInformation("Seat {SeatId} for movie show time {MovieShowTimeId} is {Status}",
                    seatId, movieShowTimeId, isReserved ? "Reserved" : "Available");

                return isReserved;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking seat {SeatId} reservation for movie show time {MovieShowTimeId}",
                    seatId, movieShowTimeId);
                throw;
            }
        }
        public async Task<List<int>> GetReservedSeatIdsAsync(int movieShowTimeId)
        {
            try
            {
                _logger.LogInformation("Getting reserved seat IDs for MovieShowTimeId {MovieShowTimeId}", movieShowTimeId);

                var reservedSeatIds = await _context.BookingSeat
                    .AsNoTracking()
                    .Where(bs => bs.Booking.MovieShowTimeId == movieShowTimeId &&
                                bs.Booking.Status != BookingStatus.Cancelled &&
                                bs.Booking.Status != BookingStatus.Expired &&
                                (bs.Booking.Status == BookingStatus.Confirmed ||
                                 (bs.Booking.Status == BookingStatus.Pending &&
                                  bs.Booking.ExpiresAt.HasValue &&
                                  bs.Booking.ExpiresAt.Value > DateTime.UtcNow)))
                    .Select(bs => bs.SeatId)
                    .Distinct()
                    .ToListAsync();

                _logger.LogInformation("Found {Count} reserved seats for MovieShowTimeId {MovieShowTimeId}",
                    reservedSeatIds.Count, movieShowTimeId);

                return reservedSeatIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reserved seat IDs for MovieShowTimeId {MovieShowTimeId}", movieShowTimeId);
                throw;
            }
        }
    }
}
