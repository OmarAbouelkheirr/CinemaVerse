using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.Ticket.Response;
using CinemaVerse.Services.Interfaces.User;
using CinemaVerse.Services.Mappers;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;


namespace CinemaVerse.Services.Implementations.User
{
    public class TicketService : ITicketService
    {
        private readonly ILogger<TicketService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public TicketService(IUnitOfWork UnitOfWork, ILogger<TicketService> Logger)
        {
            _unitOfWork = UnitOfWork;
            _logger = Logger;
        }

        private static string GenerateSecureQrToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        public async Task<IEnumerable<TicketDetailsDto>> IssueTicketsAsync(int BookingId)
        {
            try
            {
                //Input Validation
                _logger.LogInformation("Issuing ticket for Booking ID: {BookingId}", BookingId);
                if (BookingId <= 0)
                    throw new ArgumentException("Booking ID must be a positive integer.", nameof(BookingId));
                var Booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(BookingId);
                if (Booking is null)
                    throw new KeyNotFoundException("Booking not found.");
                if (Booking.Status != BookingStatus.Confirmed)
                    throw new InvalidOperationException("Booking is not confirmed.");

                // for idempotency check - avoid re-issuing tickets for already issued seats
                var issuedSeatIds = await _unitOfWork.Tickets.GetIssuedSeatIdsAsync(BookingId);
                var seatsToIssue = Booking.BookingSeats.Where(bs => !issuedSeatIds.Contains(bs.SeatId)).ToList();
                if (!seatsToIssue.Any())
                {
                    _logger.LogInformation(
                        "All tickets already issued for Booking ID: {BookingId}", BookingId);

                    return Enumerable.Empty<TicketDetailsDto>();
                }

                // Logic to issue ticket goes here
                await _unitOfWork.BeginTransactionAsync();
                var CreatedTickets = new List<Ticket>();
                foreach (var seat in seatsToIssue)
                {
                    var Ticket = new Ticket
                    {

                        TicketNumber = GenerateTicketNumber(),
                        BookingId = Booking.Id,
                        SeatId = seat.SeatId,
                        Price = Booking.MovieShowTime.Price,
                        CreatedAt = DateTime.UtcNow,
                        Status = TicketStatus.Active,
                        QrToken = GenerateSecureQrToken()
                    };
                    await _unitOfWork.Tickets.AddAsync(Ticket);
                    CreatedTickets.Add(Ticket);
                }
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully issued {TicketCount} tickets for Booking ID: {BookingId}", CreatedTickets.Count, BookingId);

                var TicketDtos = new List<TicketDetailsDto>();
                foreach (var ticket in CreatedTickets)
                {
                    TicketDtos.Add(MapToDto(ticket));
                }
                await _unitOfWork.CommitTransactionAsync();
                return TicketDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing tickets for Booking ID: {BookingId}", BookingId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
        private string GenerateTicketNumber()
        {
            _logger.LogInformation("Generating Ticket Number ");
            string Prefix = "tk-";
            string DatePart = DateTime.UtcNow.ToString("yyyyMMdd");
            string uniquePart = Guid.NewGuid().ToString("N")[..6].ToUpper();
            return $"{Prefix}-{DatePart}-{uniquePart}";
        }
        public TicketDetailsDto MapToDto(Ticket ticket) => TicketMapper.ToTicketDetailsDto(ticket);

        public async Task<PagedResultDto<TicketDetailsDto>> GetUserTicketsAsync(int userId, AdminTicketFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting tickets for user {UserId} with filter: {@Filter}", userId, filter);

                if (userId <= 0)
                    throw new ArgumentException("User ID must be a positive integer.", nameof(userId));
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found.");

                // Validate pagination
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > PaginationConstants.MaxPageSize)
                    filter.PageSize = PaginationConstants.DefaultPageSize;

                // Build query - get tickets through bookings
                var query = _unitOfWork.Tickets.GetQueryable()
                    .Where(t => t.Booking.UserId == userId);

                // Apply filters
                if (filter.Status.HasValue)
                {
                    query = query.Where(t => t.Status == filter.Status.Value);
                }

                if (filter.BookingId.HasValue)
                {
                    query = query.Where(t => t.BookingId == filter.BookingId.Value);
                }

                if (filter.ShowtimeId.HasValue)
                {
                    query = query.Where(t => t.Booking.MovieShowTimeId == filter.ShowtimeId.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.TicketNumber))
                {
                    query = query.Where(t => t.TicketNumber.Contains(filter.TicketNumber));
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(t => t.Booking.MovieShowTime.ShowStartTime >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(t => t.Booking.MovieShowTime.ShowStartTime <= filter.EndDate.Value);
                }

                // Get total count
                var totalCount = await _unitOfWork.Tickets.CountAsync(query);

                // Apply sorting (default by showtime)
                query = query.OrderByDescending(t => t.Booking.MovieShowTime.ShowStartTime);

                // Get paged results
                var tickets = await _unitOfWork.Tickets.GetPagedAsync(
                    query: query,
                    orderBy: null,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: "Booking.MovieShowTime.Movie,Booking.MovieShowTime.Movie.MovieImages,Booking.MovieShowTime.Hall,Booking.MovieShowTime.Hall.Branch,Seat"
                );

                // Map to DTOs
                var ticketDtos = tickets.Select(TicketMapper.ToTicketDetailsDto).ToList();

                _logger.LogInformation("Retrieved {Count} tickets for user {UserId} out of {Total} total",
                    ticketDtos.Count, userId, totalCount);

                return new PagedResultDto<TicketDetailsDto>
                {
                    Items = ticketDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets for user {UserId}", userId);
                throw;
            }
        }

        public async Task<TicketDetailsDto> GetUserTicketByIdAsync(int userId, int ticketId)
        {
            try
            {
                _logger.LogInformation("Getting ticket details for TicketId: {TicketId}, UserId: {UserId}", ticketId, userId);

                if (userId <= 0)
                    throw new ArgumentException("User ID must be a positive integer.", nameof(userId));
                if (ticketId <= 0)
                    throw new ArgumentException("Ticket ID must be a positive integer.", nameof(ticketId));
                var ticket = await _unitOfWork.Tickets.GetTicketWithDetailsAsync(ticketId);
                if (ticket == null)
                    throw new KeyNotFoundException($"Ticket with ID {ticketId} not found.");
                if (ticket.Booking?.UserId != userId)
                    throw new UnauthorizedAccessException("You are not authorized to access this ticket.");

                var ticketDto = MapToDto(ticket);
                _logger.LogInformation("Successfully retrieved ticket details for TicketId: {TicketId}, UserId: {UserId}", ticketId, userId);
                return ticketDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket details for TicketId: {TicketId}, UserId: {UserId}", ticketId, userId);
                throw;
            }
        }
    }
}
