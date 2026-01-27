using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminTicketService : IAdminTicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminTicketService> _logger;
        private readonly ITicketService _ticketService;

        public AdminTicketService(IUnitOfWork unitOfWork, ILogger<AdminTicketService> logger, ITicketService ticketService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _ticketService = ticketService;
        }

        public Task<bool> CancelTicketAsync(int ticketId, string cancellationReason)
        {
            throw new NotImplementedException();
        }

        //AI
        public async Task<PagedResultDto<AdminTicketListItemDto>> GetAllTicketsAsync(AdminTicketFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting all tickets with filter: {@Filter}", filter);

                if (filter == null)
                {
                    _logger.LogWarning("AdminTicketFilterDto is null");
                    throw new ArgumentNullException(nameof(filter), "AdminTicketFilterDto cannot be null.");
                }

                // Validate pagination
                if (filter.PageNumber <= 0)
                    filter.PageNumber = 1;

                if (filter.PageSize <= 0 || filter.PageSize > 100)
                    filter.PageSize = 20;

                var query = _unitOfWork.Tickets.GetQueryable();

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

                if (filter.UserId.HasValue)
                {
                    query = query.Where(t => t.Booking.UserId == filter.UserId.Value);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt <= filter.EndDate.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.TicketNumber))
                {
                    query = query.Where(t => t.TicketNumber.Contains(filter.TicketNumber));
                }

                var totalCount = await _unitOfWork.Tickets.CountAsync(query);

                Func<IQueryable<Ticket>, IOrderedQueryable<Ticket>> orderByFunc = q =>
                    q.OrderByDescending(t => t.CreatedAt);

                var tickets = await _unitOfWork.Tickets.GetPagedAsync(
                    query: query,
                    orderBy: orderByFunc,
                    skip: (filter.PageNumber - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: "Booking.User,Booking.MovieShowTime.Movie.MovieImages,Booking.MovieShowTime.Hall.Branch,Seat"
                );

                var ticketDtos = new List<AdminTicketListItemDto>();
                foreach (var ticket in tickets)
                {
                    var baseDto = _ticketService.MapToDto(ticket);

                    var adminDto = new AdminTicketListItemDto
                    {
                        TicketId = baseDto.TicketId,
                        TicketNumber = baseDto.TicketNumber,
                        MovieName = baseDto.MovieName,
                        ShowStartTime = baseDto.ShowStartTime,
                        Status = baseDto.Status,
                        Price = baseDto.Price,
                        BranchName = baseDto.BranchName,
                        UserEmail = ticket.Booking?.User?.Email ?? string.Empty,
                        BookingStatus = ticket.Booking?.Status ?? BookingStatus.Pending
                    };

                    ticketDtos.Add(adminDto);
                }

                _logger.LogInformation("Retrieved {Count} tickets out of {Total} total",
                    ticketDtos.Count, totalCount);

                return new PagedResultDto<AdminTicketListItemDto>
                {
                    Items = ticketDtos,
                    Page = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tickets");
                throw;
            }
        }


        public async Task<AdminTicketDetailsDto?> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                _logger.LogInformation("Getting ticket with ID {TicketId}", ticketId);

                if (ticketId <= 0)
                {
                    _logger.LogWarning("Invalid ticketId: {TicketId}", ticketId);
                    throw new ArgumentException("Ticket ID must be greater than zero.", nameof(ticketId));
                }

                var ticket = await _unitOfWork.Tickets.GetTicketWithDetailsAsync(ticketId);

                if (ticket == null)
                {
                    _logger.LogWarning("Ticket with ID {TicketId} not found", ticketId);
                    return null;
                }

                var baseDto = _ticketService.MapToDto(ticket);

                var adminDto = new AdminTicketDetailsDto
                {
                    // Base properties from TicketDetailsDto
                    TicketId = baseDto.TicketId,
                    TicketNumber = baseDto.TicketNumber,
                    MovieName = baseDto.MovieName,
                    ShowStartTime = baseDto.ShowStartTime,
                    MovieDuration = baseDto.MovieDuration,
                    HallNumber = baseDto.HallNumber,
                    HallType = baseDto.HallType,
                    SeatLabel = baseDto.SeatLabel,
                    MoviePoster = baseDto.MoviePoster,
                    MovieAgeRating = baseDto.MovieAgeRating,
                    QrToken = baseDto.QrToken,
                    Status = baseDto.Status,
                    Price = baseDto.Price,
                    BranchName = baseDto.BranchName,

                    // Admin-specific properties (with null checks)
                    UserId = ticket.Booking?.UserId ?? 0,
                    UserEmail = ticket.Booking?.User?.Email ?? string.Empty,
                    FullName = ticket.Booking?.User?.FullName ?? string.Empty,
                    BookingId = ticket.BookingId,
                    BookingStatus = ticket.Booking?.Status ?? BookingStatus.Pending,
                    UsedAt = ticket.Status == TicketStatus.Used ? ticket.CreatedAt : null
                };

                _logger.LogInformation("Successfully retrieved ticket {TicketId}", ticketId);
                return adminDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket with ID {TicketId}", ticketId);
                throw;
            }
        }

        public Task<AdminTicketDetailsDto?> GetTicketByNumberAsync(string ticketNumber)
        {
            throw new NotImplementedException();
        }

        public Task<AdminTicketDetailsDto?> GetTicketByQrTokenAsync(string qrToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<AdminTicketDetailsDto>> GetTicketsByBookingIdAsync(int bookingId)
        {
            throw new NotImplementedException();
        }

        public Task<List<AdminTicketDetailsDto>> GetTicketsByShowtimeIdAsync(int showtimeId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkTicketAsUsedAsync(int ticketId, string? adminNote = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MarkTicketAsUsedByQrTokenAsync(string qrToken, string? adminNote = null)
        {
            throw new NotImplementedException();
        }
    }
}