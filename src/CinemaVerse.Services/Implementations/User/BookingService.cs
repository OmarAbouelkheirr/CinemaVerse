using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.Payment.Requests;
using CinemaVerse.Services.DTOs.Ticket.Response;
using CinemaVerse.Services.Interfaces.User;
using CinemaVerse.Services.Mappers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace CinemaVerse.Services.Implementations.User
{
    public class BookingService : IBookingService
    {
        private readonly ILogger<BookingService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITicketService _ticketService;
        private readonly IPaymentService _paymentService;
        private readonly IEmailService _emailService;

        public BookingService(ILogger<BookingService> logger, IUnitOfWork unitOfWork, ITicketService ticketService, IPaymentService paymentService, IEmailService emailService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _ticketService = ticketService;
            _paymentService = paymentService;
            _emailService = emailService;
        }
        public async Task<bool> CancelUserBookingAsync(int userId, int bookingId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Cancelling booking {BookingId} for UserId {UserId}", bookingId, userId);

                if (bookingId <= 0)
                    throw new ArgumentException("Booking ID must be a positive integer.", nameof(bookingId));
                if (userId <= 0)
                    throw new ArgumentException("User ID must be a positive integer.", nameof(userId));

                var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);
                if (booking == null)
                    throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                if (booking.UserId != userId)
                    throw new UnauthorizedAccessException($"You are not authorized to confirm booking {bookingId}.");
                if (booking.Status == BookingStatus.Cancelled)
                    throw new InvalidOperationException($"Booking {bookingId} is already cancelled.");
                if (booking.Status == BookingStatus.Confirmed && booking.MovieShowTime?.ShowStartTime <= DateTime.UtcNow)
                    throw new InvalidOperationException($"Cannot cancel booking {bookingId} as the showtime has already started.");
                if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
                    throw new InvalidOperationException($"Cannot cancel booking {bookingId}. Only Pending or Confirmed bookings can be cancelled. Current status: {booking.Status}.");


                // step number one - update status
                await _unitOfWork.Bookings.UpdateBookingStatusAsync(bookingId, BookingStatus.Cancelled);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully cancelled booking {BookingId} for UserId {UserId} (step number one - update status)", bookingId, userId);

                var completedPayment = booking.BookingPayments?.FirstOrDefault(bp =>
                    bp.Status == PaymentStatus.Completed &&
                    !string.IsNullOrEmpty(bp.PaymentIntentId));

                if (completedPayment != null)
                {
                    var refundRequest = new RefundPaymentRequestDto
                    {
                        PaymentIntentId = completedPayment.PaymentIntentId,
                        BookingId = bookingId,
                        RefundAmount = completedPayment.Amount
                    };

                    // step number two - refund payment
                    // Note: RefundPaymentAsync does NOT manage its own transaction
                    // It uses the existing transaction from CancelUserBookingAsync
                    // If refund fails, the entire transaction will rollback
                    try
                    {
                        var refundResult = await _paymentService.RefundPaymentAsync(refundRequest);

                        if (!refundResult)
                        {
                            _logger.LogError("Failed to process refund for BookingId {BookingId} after cancellation for UserId {UserId}", bookingId, userId);
                            throw new InvalidOperationException($"Failed to process refund for booking {bookingId}.");
                        }

                        _logger.LogInformation("Successfully processed refund for booking {BookingId} for UserId {UserId} (step number two - refund payment)", bookingId, userId);
                    }
                    catch (Exception refundEx)
                    {
                        // If refund fails, rollback the booking cancellation
                        _logger.LogError(refundEx, "Refund failed for BookingId {BookingId}, rolling back booking cancellation", bookingId);
                        throw; // This will trigger the catch block below to rollback
                    }
                }

                await _unitOfWork.CommitTransactionAsync();


                // step number three - send email (after refund succeeds)
                // Email should be sent outside transaction or in a try-catch that doesn't rollback

                if (booking.User.IsEmailConfirmed)
                {
                    try
                    {
                        var emailRequest = BookingEmailDtoMapper.ToCancellationEmailDto(
                            booking,
                            completedPayment?.Amount ?? 0,
                            "User Requested Cancellation");
                        await _emailService.SendBookingCancellationEmailAsync(emailRequest);
                        _logger.LogInformation("Cancellation email sent for BookingId {BookingId} (step number three - send email)", bookingId);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send cancellation email for BookingId {BookingId}, but cancellation was successful", bookingId);
                    }
                }

                return true;

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error cancelling booking {BookingId} for UserId {UserId}", bookingId, userId);
                throw;
            }
        }

        public async Task<BookingDetailsDto> ConfirmBookingAsync(int userId, int bookingId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Confirm booking {BookingId} for UserId {UserId}", bookingId, userId);

                if (bookingId <= 0)
                    throw new ArgumentException("Booking ID must be a positive integer.", nameof(bookingId));
                if (userId <= 0)
                    throw new ArgumentException("User ID must be a positive integer.", nameof(userId));

                var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);
                if (booking == null)
                    throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                if (booking.UserId != userId)
                    throw new UnauthorizedAccessException($"You are not authorized to confirm booking {bookingId}.");
                if (booking.Status != BookingStatus.Pending)
                {
                    // Idempotency: if already confirmed, return existing booking details
                    if (booking.Status == BookingStatus.Confirmed)
                    {
                        _logger.LogInformation("Booking {BookingId} already confirmed, issuing tickets if needed", bookingId);

                        // Issue tickets if needed (idempotent operation)
                        var confirmedTickets = await _ticketService.IssueTicketsAsync(bookingId);

                        // Refresh booking to get latest data
                        booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);
                        if (booking == null)
                            throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                        return BookingMapper.ToDetailsDto(booking, confirmedTickets);
                    }
                    else
                        throw new InvalidOperationException($"Only bookings in Pending status can be confirmed. Current status: {booking.Status}.");
                }

                await _unitOfWork.Bookings.UpdateBookingStatusAsync(bookingId, BookingStatus.Confirmed);
                await _unitOfWork.SaveChangesAsync();

                // Issue Tickets
                var tickets = await _ticketService.IssueTicketsAsync(bookingId);

                // Refresh booking to get updated status
                booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);
                if (booking == null)
                    throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                // Build BookingDetailsDto
                var bookingDetails = BookingMapper.ToDetailsDto(booking, tickets);

                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Successfully confirmed booking {BookingId} and issued {TicketCount} tickets for UserId {UserId}",
                    bookingId, tickets.Count(), userId);

                if (booking.User?.IsEmailConfirmed == true)
                {
                    try
                    {
                        var emailRequest = BookingEmailDtoMapper.ToConfirmationEmailDto(booking);
                        await _emailService.SendBookingConfirmationEmailAsync(emailRequest);
                        _logger.LogInformation("Booking confirmation email sent for BookingId {BookingId}", bookingId);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send booking confirmation email for BookingId {BookingId}, but booking was confirmed", bookingId);
                    }
                }

                return bookingDetails;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error confirming booking {BookingId} for UserId {UserId}", bookingId, userId);
                throw;
            }
        }

        public async Task<BookingDetailsDto> CreateBookingAsync(CreateBookingRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                _logger.LogInformation("Creating booking for UserId {UserId}", request.UserId);

                var movieShowTime = await _unitOfWork.MovieShowTimes.GetMovieShowTimeWithDetailsAsync(request.MovieShowTimeId);
                if (movieShowTime == null)
                    throw new KeyNotFoundException($"MovieShowTime with ID {request.MovieShowTimeId} not found.");
                if (movieShowTime.Hall == null)
                    throw new InvalidOperationException($"Hall information is missing for MovieShowTime {request.MovieShowTimeId}.");
                if (movieShowTime.Hall.HallStatus == HallStatus.Maintenance || movieShowTime.Hall.HallStatus == HallStatus.Closed)
                    throw new InvalidOperationException("The hall is not available for booking at the moment.");
                if (movieShowTime.ShowStartTime <= DateTime.UtcNow)
                    throw new InvalidOperationException("Cannot book seats for a showtime that has already started.");

                var hallSeatIds = movieShowTime.Hall.Seats?.Select(s => s.Id).ToList() ?? new List<int>();
                var invalidSeats = request.SeatIds.Where(seatId => !hallSeatIds.Contains(seatId)).ToList();
                if (invalidSeats.Any())
                    throw new ArgumentException($"SeatIds {string.Join(", ", invalidSeats)} do not exist in this hall.");
                var duplicateSeats = request.SeatIds.GroupBy(s => s).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicateSeats.Any())
                    throw new ArgumentException($"Duplicate SeatIds found: {string.Join(", ", duplicateSeats)}");

                var reservedSeatIds = await _unitOfWork.MovieShowTimes.GetReservedSeatIdsAsync(request.MovieShowTimeId);
                var reservedSet = reservedSeatIds.ToHashSet();
                var alreadyReserved = request.SeatIds.Where(seatId => reservedSet.Contains(seatId)).ToList();
                if (alreadyReserved.Any())
                    throw new InvalidOperationException($"SeatId {string.Join(", ", alreadyReserved)} is already reserved for the selected showtime.");

                var totalAmount = (movieShowTime.Price * request.SeatIds.Count) * 1.14m;

                var booking = new Booking
                {
                    UserId = request.UserId,
                    MovieShowTimeId = request.MovieShowTimeId,
                    Status = BookingStatus.Pending,
                    TotalAmount = totalAmount,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(BookingConstants.PendingBookingExpiryMinutes)
                };

                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.SaveChangesAsync(); // Save to get Booking.Id

                foreach (var seatId in request.SeatIds)
                {
                    var bookingSeat = new BookingSeat
                    {
                        BookingId = booking.Id,
                        SeatId = seatId
                    };
                    await _unitOfWork.BookingSeat.AddAsync(bookingSeat);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully created booking {BookingId} for UserId {UserId} with {SeatCount} seats",
                    booking.Id, request.UserId, request.SeatIds.Count);

                // Reload booking with all details for mapping
                var bookingWithDetails = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(booking.Id);

                // Build BookingDetailsDto (Tickets empty until payment confirmed)
                return BookingMapper.ToDetailsDto(bookingWithDetails ?? booking, new List<TicketDetailsDto>());
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                if (IsConcurrencyOrDeadlockException(ex))
                    throw new InvalidOperationException("One or more selected seats were just booked by another user. Please refresh and try again.");

                _logger.LogError(ex, "Error creating booking for UserId {UserId}", request.UserId);
                throw;
            }
        }

        private static bool IsConcurrencyOrDeadlockException(Exception ex)
        {
            var sqlEx = ex as SqlException ?? (ex as DbUpdateException)?.InnerException as SqlException;
            if (sqlEx == null && ex.InnerException != null)
                sqlEx = ex.InnerException as SqlException ?? (ex.InnerException as DbUpdateException)?.InnerException as SqlException;
            if (sqlEx == null)
                return false;
            return sqlEx.Number == 1205 || sqlEx.Number == 3960 || sqlEx.Number == 3961;
        }

        public async Task<BookingDetailsDto> GetUserBookingByIdAsync(int userId, int bookingId)
        {
            try
            {
                _logger.LogInformation("Getting BookingId {BookingId} for UserId {UserId}", bookingId, userId);

                // 1. Validate inputs
                if (userId <= 0)
                    throw new ArgumentException("User ID must be a positive integer.", nameof(userId));
                if (bookingId <= 0)
                    throw new ArgumentException("Booking ID must be a positive integer.", nameof(bookingId));

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);
                if (booking == null)
                    throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                if (booking.UserId != userId)
                    throw new UnauthorizedAccessException($"You are not authorized to access booking {bookingId}.");

                var ticketDtos = new List<TicketDetailsDto>();
                if (booking.Tickets != null && booking.Tickets.Any())
                {
                    foreach (var ticket in booking.Tickets)
                    {
                        ticketDtos.Add(_ticketService.MapToDto(ticket));
                    }
                }

                var bookingDetails = BookingMapper.ToDetailsDto(booking, ticketDtos);

                _logger.LogInformation("Successfully retrieved booking {BookingId} for UserId {UserId}", bookingId, userId);
                return bookingDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting BookingId {BookingId} for UserId {UserId}", bookingId, userId);
                throw;
            }
        }

        public async Task<PagedResultDto<BookingDetailsDto>> GetUserBookingsAsync(int userId, AdminBookingFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting bookings for user {UserId} with filter: {@Filter}", userId, filter);

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

                // Build query - filter by userId
                var query = _unitOfWork.Bookings.GetQueryable()
                    .Where(b => b.UserId == userId);

                // Apply additional filters
                if (filter.Status.HasValue)
                {
                    query = query.Where(b => b.Status == filter.Status.Value);
                }

                if (filter.MovieShowTimeId.HasValue)
                {
                    query = query.Where(b => b.MovieShowTimeId == filter.MovieShowTimeId.Value);
                }

                if (filter.CreatedFrom.HasValue)
                {
                    query = query.Where(b => b.CreatedAt >= filter.CreatedFrom.Value);
                }

                if (filter.CreatedTo.HasValue)
                {
                    query = query.Where(b => b.CreatedAt <= filter.CreatedTo.Value);
                }

                if (filter.MinAmount.HasValue)
                {
                    query = query.Where(b => b.TotalAmount >= filter.MinAmount.Value);
                }

                if (filter.MaxAmount.HasValue)
                {
                    query = query.Where(b => b.TotalAmount <= filter.MaxAmount.Value);
                }

                // Get total count
                var totalCount = await _unitOfWork.Bookings.CountAsync(query);

                // Apply sorting
                string sortBy = filter.SortBy?.ToLower() ?? "createdat";
                string sortOrder = filter.SortOrder?.ToLower() ?? "desc";

                if (sortBy == "createdat")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.CreatedAt)
                        : query.OrderByDescending(b => b.CreatedAt);
                }
                else if (sortBy == "totalamount")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.TotalAmount)
                        : query.OrderByDescending(b => b.TotalAmount);
                }
                else if (sortBy == "status")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.Status)
                        : query.OrderByDescending(b => b.Status);
                }
                else
                {
                    query = query.OrderByDescending(b => b.CreatedAt);
                }

                // Get paged results with related entities
                var bookings = await _unitOfWork.Bookings.GetPagedAsync(
                    query: query,
                    orderBy: null,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: "User,MovieShowTime.Movie.MovieImages,BookingSeats.Seat,Tickets"
                );

                // Map to DTOs
                var bookingDtos = bookings.Select(b => BookingMapper.ToDetailsDto(b)).ToList();

                _logger.LogInformation("Retrieved {Count} bookings for user {UserId} out of {Total} total",
                    bookingDtos.Count, userId, totalCount);

                return new PagedResultDto<BookingDetailsDto>
                {
                    Items = bookingDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user {UserId}", userId);
                throw;
            }
        }


    }
}
