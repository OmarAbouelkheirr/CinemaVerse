using CinemaVerse.Data.Data;
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
    public class TicketsRepository : Repository<Ticket>, ITicketsRepository
    {

        public TicketsRepository(AppDbContext context, ILogger<Ticket> logger)
            : base(context, logger)
        {}

        public async Task<Ticket?> GetByTicketNumberAsync(string TicketNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TicketNumber))
                {
                    _logger.LogWarning("Invalid TicketNumber provided: {TicketNumber}", TicketNumber);
                    throw new ArgumentException("TicketNumber cannot be null or empty", nameof(TicketNumber));
                }
                _logger.LogInformation("Getting ticket by ticket number {TicketNumber}", TicketNumber);
                var Result = await _dbSet
                .Include(t => t.Seat)
                .Include(t => t.Booking)
                .FirstOrDefaultAsync(t => t.TicketNumber == TicketNumber);
                if (Result == null)
                {
                    _logger.LogWarning("Ticket with ticket number {TicketNumber} not found", TicketNumber);
                }
                else
                {
                    _logger.LogInformation("Ticket with ticket number {TicketNumber} retrieved successfully", TicketNumber);
                }
                return Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error getting ticket by ticket number {TicketNumber}", TicketNumber);
                throw;
            }
        }

        public async Task<Ticket?> GetTicketWithDetailsAsync(int TicketId)
        {
            try
            {
                _logger.LogInformation("Getting ticket {TicketId} with details", TicketId);
                var Result = await _dbSet
                    .AsNoTracking()
                    .Include(t => t.Seat)
                        .ThenInclude(s=>s.Hall)
                    .Include(t => t.Booking)
                        .ThenInclude(b => b.User)
                    .Include(t => t.Booking)
                        .ThenInclude(b => b.MovieShowTime)
                            .ThenInclude(ms => ms.Movie)
                    .FirstOrDefaultAsync(t => t.Id == TicketId);
                if (Result == null)
                {
                    _logger.LogWarning("Ticket with ticket Id {TicketId} not found", TicketId);
                }
                else
                {
                    _logger.LogInformation("Ticket with ticket Id {TicketId} retrieved successfully", TicketId);
                }
                return Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket by ticket Id {TicketId}", TicketId);
                throw;
            }
        }

        public async Task<IEnumerable<Ticket>> GetUserTicketsAsync(Guid UserId)
        {
            try
            {
                _logger.LogInformation("Getting user tickets for user {UserId}",UserId);
                var Result = await _dbSet
                    .AsNoTracking()
                    .Where(t => t.Booking.UserId == UserId)
                    .Include(t => t.Booking)
                        .ThenInclude(b => b.MovieShowTime)
                        .ThenInclude(ms => ms.Movie)
                    .Include(t => t.Seat)
                    .OrderByDescending(t => t.Booking.CreatedAt)
                    .ToListAsync();

                _logger.LogDebug("Retrieved {Count} tickets for user {UserId}", Result.Count, UserId);
                return Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user tickets for user {UserId}", UserId);
                throw;
            }
        }
    }
}
