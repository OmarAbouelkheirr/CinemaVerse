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


        public Task<bool> CancelTicketAsync(int ticketId)
        {
            throw new NotImplementedException();
        }

        public async Task<TicketCheckResultDto> CheckTicketByQrTokenAsync(string qrToken)
        {
            try
            {
                _logger.LogInformation("Checking ticket by QR token {QrToken}", qrToken);

                if (string.IsNullOrWhiteSpace(qrToken))
                {
                    _logger.LogWarning("QR token is null or empty");
                    throw new ArgumentException("QR token cannot be null or empty.", nameof(qrToken));
                }

                var ticket = await _unitOfWork.Tickets.GetTicketByQrTokenAsync(qrToken);

                if (ticket == null)
                {
                    _logger.LogWarning("Ticket with QR token {QrToken} not found", qrToken);

                    return new TicketCheckResultDto
                    {
                        IsFound = false,
                        Message = "Ticket not found."
                    };
                }

                var baseDto = _ticketService.MapToDto(ticket);

                var result = new TicketCheckResultDto
                {
                    TicketNumber = baseDto.TicketNumber,
                    Status = baseDto.Status,
                    MovieName = baseDto.MovieName,
                    ShowStartTime = baseDto.ShowStartTime,
                    MovieDuration = baseDto.MovieDuration,
                    HallNumber = baseDto.HallNumber,
                    HallType = baseDto.HallType,
                    SeatLabel = baseDto.SeatLabel,
                    Price = baseDto.Price,
                    BranchName = baseDto.BranchName,
                    IsFound = true,
                    Message = "Ticket found."
                };

                _logger.LogInformation("Successfully checked ticket by QR token {QrToken}", qrToken);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking ticket by QR token {QrToken}", qrToken);
                throw;
            }
        }

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
                        UserFullName = ticket.Booking?.User?.FullName ?? string.Empty,
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

        public async Task<List<AdminTicketDetailsDto>> GetTicketsByBookingIdAsync(int bookingId)
        {
            try
            {
                _logger.LogInformation("Getting tickets for booking ID {BookingId}", bookingId);

                if (bookingId <= 0)
                {
                    _logger.LogWarning("Invalid bookingId: {BookingId}", bookingId);
                    throw new ArgumentException("Booking ID must be greater than zero.", nameof(bookingId));
                }

                var query = _unitOfWork.Tickets.GetQueryable()
                    .Where(t => t.BookingId == bookingId);

                var tickets = await _unitOfWork.Tickets.GetPagedAsync(
                    query: query,
                    orderBy: q => q.OrderBy(t => t.CreatedAt),
                    skip: 0,
                    take: int.MaxValue, // Get all tickets for this booking
                    includeProperties: "Booking.User,Booking.MovieShowTime.Movie.MovieImages,Booking.MovieShowTime.Hall.Branch,Seat"
                );

                if (tickets == null || !tickets.Any())
                {
                    _logger.LogWarning("No tickets found for booking ID {BookingId}", bookingId);
                    return new List<AdminTicketDetailsDto>();
                }

                var ticketDetails = new List<AdminTicketDetailsDto>();

                foreach (var ticket in tickets)
                {
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

                    ticketDetails.Add(adminDto);
                }

                _logger.LogInformation("Successfully retrieved {Count} tickets for booking ID {BookingId}",
                    ticketDetails.Count, bookingId);

                return ticketDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets for booking ID {BookingId}", bookingId);
                throw;
            }
        }

        public async Task<List<AdminTicketDetailsDto>> GetTicketsByShowtimeIdAsync(int showtimeId)
        {
            try
            {
                _logger.LogInformation("Getting tickets for showtime ID {ShowtimeId}", showtimeId);

                if (showtimeId <= 0)
                {
                    _logger.LogWarning("Invalid showtimeId: {ShowtimeId}", showtimeId);
                    throw new ArgumentException("Showtime ID must be greater than zero.", nameof(showtimeId));
                }

                var query = _unitOfWork.Tickets.GetQueryable()
                    .Where(t => t.Booking.MovieShowTimeId == showtimeId);

                var tickets = await _unitOfWork.Tickets.GetPagedAsync(
                    query: query,
                    orderBy: q => q.OrderBy(t => t.CreatedAt),
                    skip: 0,
                    take: int.MaxValue, // Get all tickets for this showtime
                    includeProperties: "Booking.User,Booking.MovieShowTime.Movie.MovieImages,Booking.MovieShowTime.Hall.Branch,Seat"
                );

                if (tickets == null || !tickets.Any())
                {
                    _logger.LogWarning("No tickets found for showtime ID {ShowtimeId}", showtimeId);
                    return new List<AdminTicketDetailsDto>();
                }

                var ticketDetails = new List<AdminTicketDetailsDto>();

                foreach (var ticket in tickets)
                {
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

                    ticketDetails.Add(adminDto);
                }

                _logger.LogInformation("Successfully retrieved {Count} tickets for showtime ID {ShowtimeId}",
                    ticketDetails.Count, showtimeId);

                return ticketDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tickets for showtime ID {ShowtimeId}", showtimeId);
                throw;
            }
        }

        public async Task<TicketCheckInResultDto> MarkTicketAsUsedAsync(string qrToken)
        {
            try
            {
                _logger.LogInformation("Marking ticket as used by QR token {QrToken}", qrToken);

                if (string.IsNullOrWhiteSpace(qrToken))
                {
                    _logger.LogWarning("QR token is null or empty");
                    throw new ArgumentException("QR token cannot be null or empty.", nameof(qrToken));
                }

                var ticket = await _unitOfWork.Tickets.GetTicketByQrTokenAsync(qrToken);

                if (ticket == null)
                {
                    _logger.LogWarning("Ticket with QR token {QrToken} not found", qrToken);

                    return new TicketCheckInResultDto
                    {
                        Success = false,
                        Result = TicketCheckInResult.NotFound,
                        Message = "Ticket not found."
                    };
                }

                if (ticket.Status == TicketStatus.Used)
                {
                    _logger.LogWarning("Ticket with QR token {QrToken} is already used", qrToken);

                    return new TicketCheckInResultDto
                    {
                        Success = false,
                        Result = TicketCheckInResult.AlreadyUsed,
                        Message = "Ticket has already been used."
                    };
                }

                if (ticket.Status == TicketStatus.Cancelled)
                {
                    _logger.LogWarning("Ticket with QR token {QrToken} is cancelled", qrToken);

                    return new TicketCheckInResultDto
                    {
                        Success = false,
                        Result = TicketCheckInResult.Cancelled,
                        Message = "Ticket is cancelled."
                    };
                }

                if (ticket.Status != TicketStatus.Active)
                {
                    _logger.LogWarning(
                        "Ticket with QR token {QrToken} is in invalid status {Status} for check-in",
                        qrToken, ticket.Status);

                    return new TicketCheckInResultDto
                    {
                        Success = false,
                        Result = TicketCheckInResult.InvalidStatus,
                        Message = $"Ticket is in invalid status: {ticket.Status}."
                    };
                }

                _logger.LogInformation("Updating ticket with QR token {QrToken} status to Used", qrToken);

                ticket.Status = TicketStatus.Used;
                await _unitOfWork.Tickets.UpdateAsync(ticket);
                var rows = await _unitOfWork.SaveChangesAsync();

                if (rows > 0)
                {
                    _logger.LogInformation("Ticket with QR token {QrToken} marked as used successfully", qrToken);

                    return new TicketCheckInResultDto
                    {
                        Success = true,
                        Result = TicketCheckInResult.Success,
                        Message = "Ticket marked as used successfully."
                    };
                }

                _logger.LogWarning("SaveChanges affected 0 rows while marking ticket with QR token {QrToken} as used", qrToken);

                return new TicketCheckInResultDto
                {
                    Success = false,
                    Result = TicketCheckInResult.Error,
                    Message = "Failed to update ticket status."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking ticket as used by QR token {QrToken}", qrToken);

                throw;
            }
        }

    }
}