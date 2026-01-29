using System.Data;
using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.UserFlow.Booking.Helpers;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.Email.Flow;
using CinemaVerse.Services.DTOs.Email.Requests;
using CinemaVerse.Services.DTOs.HallSeat.Responses;
using CinemaVerse.Services.DTOs.Payment.Requests;
using CinemaVerse.Services.DTOs.Ticket.Response;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

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
                {
                    _logger.LogWarning("Invalid Argument BookingId must be greater than zero, UserId {UserId}", userId);
                    throw new ArgumentException("BookingId must be greater than zero.", nameof(bookingId));
                }

                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid Argument UserId must be greater than zero");
                    throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
                }

                var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);

                if (booking == null)
                {
                    _logger.LogWarning("BookingId {BookingId} not found", bookingId);
                    throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                }

                if (booking.UserId != userId)
                {
                    _logger.LogWarning("Unauthorized: BookingId {BookingId} does not belong to UserId {UserId}", bookingId, userId);
                    throw new UnauthorizedAccessException($"You are not authorized to confirm booking {bookingId}.");
                }

                if (booking.Status == BookingStatus.Cancelled)
                {
                    _logger.LogWarning("BookingId {BookingId} is already cancelled for UserId {UserId}", bookingId, userId);
                    throw new InvalidOperationException($"Booking {bookingId} is already cancelled.");
                }

                if (booking.Status == BookingStatus.Confirmed)
                {
                    // Check if showtime has already started
                    if (booking.MovieShowTime?.ShowStartTime <= DateTime.UtcNow)
                    {
                        _logger.LogWarning("Cannot cancel BookingId {BookingId} - showtime has already started for UserId {UserId}", bookingId, userId);
                        throw new InvalidOperationException($"Cannot cancel booking {bookingId} as the showtime has already started.");
                    }
                }

                if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
                {
                    _logger.LogWarning("Cannot cancel BookingId {BookingId} with status {Status} for UserId {UserId}", bookingId, booking.Status, userId);
                    throw new InvalidOperationException($"Cannot cancel booking {bookingId}. Only Pending or Confirmed bookings can be cancelled. Current status: {booking.Status}.");
                }


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
                        var emailRequest = new BookingCancellationEmailDto
                        {
                            To = booking.User.Email,

                            BookingId = booking.Id,
                            FullName = booking.User.FullName,
                            MovieName = booking.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                            ShowStartTime = booking.MovieShowTime?.ShowStartTime ?? DateTime.MinValue,
                            RefundAmount = completedPayment?.Amount ?? 0,
                            Currency = "EGP",
                            CancellationReason = "User Requested Cancellation"
                        };

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
                {
                    _logger.LogWarning("Invalid Argument BookingId must be greater than zero, UserId {UserId}", userId);
                    throw new ArgumentException("BookingId must be greater than zero.", nameof(bookingId));
                }

                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid Argument UserId must be greater than zero");
                    throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
                }

                var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);

                if (booking == null)
                {
                    _logger.LogWarning("BookingId {BookingId} not found", bookingId);
                    throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                }

                if (booking.UserId != userId)
                {
                    _logger.LogWarning("Unauthorized: BookingId {BookingId} does not belong to UserId {UserId}", bookingId, userId);
                    throw new UnauthorizedAccessException($"You are not authorized to confirm booking {bookingId}.");
                }

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
                        {
                            _logger.LogWarning("booking {bookingId} not found ", bookingId);
                            throw new ArgumentException("booking is not found.");
                        }

                        return BuildBookingDetailsDto(booking, confirmedTickets);
                    }
                    else
                    {
                        _logger.LogWarning("BookingId {BookingId} is not in Pending status, current status {Status}", bookingId, booking.Status);
                        throw new InvalidOperationException($"Only bookings in Pending status can be confirmed. Current status: {booking.Status}.");
                    }
                }

                await _unitOfWork.Bookings.UpdateBookingStatusAsync(bookingId, BookingStatus.Confirmed);
                await _unitOfWork.SaveChangesAsync();

                // Issue Tickets
                var tickets = await _ticketService.IssueTicketsAsync(bookingId);

                // Refresh booking to get updated status
                booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogWarning("booking {bookingId} not found ", bookingId);
                    throw new ArgumentException("booking is not found.");
                }
                // Build BookingDetailsDto
                var bookingDetails = BuildBookingDetailsDto(booking, tickets);

                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Successfully confirmed booking {BookingId} and issued {TicketCount} tickets for UserId {UserId}",
                    bookingId, tickets.Count(), userId);

                if (booking.User?.IsEmailConfirmed == true)
                {
                    try
                    {
                        var emailRequest = new BookingConfirmationEmailDto
                        {
                            To = booking.User.Email,
                            BookingId = booking.Id,
                            FullName = booking.User.FullName,
                            MovieName = booking.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                            ShowStartTime = booking.MovieShowTime?.ShowStartTime ?? DateTime.MinValue,
                            ShowEndTime = booking.MovieShowTime?.ShowEndTime ?? DateTime.MinValue,
                            HallNumber = booking.MovieShowTime?.Hall?.HallNumber ?? string.Empty,
                            BranchName = booking.MovieShowTime?.Hall?.Branch?.BranchName ?? string.Empty,
                            TotalAmount = booking.TotalAmount,
                            Currency = "EGP",
                            Tickets = booking.Tickets?.Select(t => new TicketInfoDto
                            {
                                TicketNumber = t.TicketNumber,
                                SeatNumber = t.Seat?.SeatLabel ?? string.Empty,
                                Price = t.Price
                            }).ToList() ?? new List<TicketInfoDto>()
                        };
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

        public async Task<BookingDetailsDto> CreateBookingAsync( CreateBookingRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                //Input Validation
                _logger.LogInformation("Creating booking for UserId {UserId}", request.UserId);

                if (request.MovieShowTimeId <= 0)
                {
                    _logger.LogWarning("Invalid Argument MovieShowTimeId {MovieShowTimeId}, UserId {UserId}", request.MovieShowTimeId, request.UserId);
                    throw new ArgumentException("MovieShowTimeId must be a positive integer.", nameof(request.MovieShowTimeId));
                }
                if (request.SeatIds == null || !request.SeatIds.Any())
                {
                    _logger.LogWarning("Invalid Argument SeatIds is null or empty, UserId {UserId}", request.UserId);
                    throw new ArgumentException("SeatIds cannot be null or empty.", nameof(request.SeatIds));
                }


                var movieShowTime = await _unitOfWork.MovieShowTimes.GetMovieShowTimeWithDetailsAsync(request.MovieShowTimeId);
                if (movieShowTime == null)
                {
                    _logger.LogWarning("MovieShowTimeId {MovieShowTimeId} not found, UserId {UserId}", request.MovieShowTimeId, request.UserId);
                    throw new KeyNotFoundException($"MovieShowTime with ID {request.MovieShowTimeId} not found.");
                }


                if (movieShowTime.Hall == null)
                {
                    _logger.LogError("Hall not loaded for MovieShowTimeId {MovieShowTimeId}", request.MovieShowTimeId);
                    throw new InvalidOperationException($"Hall information is missing for MovieShowTime {request.MovieShowTimeId}.");
                }

                if (movieShowTime.Hall.HallStatus == HallStatus.Maintenance ||
                    movieShowTime.Hall.HallStatus == HallStatus.Closed)
                {
                    _logger.LogWarning("Hall {HallId} is not available at the moment", movieShowTime.Hall.Id);
                    throw new InvalidOperationException("The hall is not available for booking at the moment.");
                }


                if (movieShowTime.ShowStartTime <= DateTime.UtcNow)
                {
                    _logger.LogWarning("MovieShowTimeId {MovieShowTimeId} has already started, UserId {UserId}", request.MovieShowTimeId, request.UserId);
                    throw new InvalidOperationException("Cannot book seats for a showtime that has already started.");
                }

                var hallSeatIds = movieShowTime.Hall.Seats?.Select(s => s.Id).ToList() ?? new List<int>();
                var invalidSeats = request.SeatIds.Where(seatId => !hallSeatIds.Contains(seatId)).ToList();
                if (invalidSeats.Any())
                {
                    _logger.LogWarning("Invalid SeatIds {SeatIds} do not exist in Hall {HallId}, UserId {UserId}",
                        string.Join(", ", invalidSeats), movieShowTime.Hall.Id, request.UserId);
                    throw new ArgumentException($"SeatIds {string.Join(", ", invalidSeats)} do not exist in this hall.");
                }

                var duplicateSeats = request.SeatIds.GroupBy(s => s).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicateSeats.Any())
                {
                    _logger.LogWarning("Duplicate SeatIds {SeatIds} in request, UserId {UserId}",
                        string.Join(", ", duplicateSeats), request.UserId);
                    throw new ArgumentException($"Duplicate SeatIds found: {string.Join(", ", duplicateSeats)}");
                }

                // Check seats availability (must be done in transaction to prevent race conditions)
                foreach (var seatId in request.SeatIds)
                {
                    bool seatReserved = await _unitOfWork.MovieShowTimes.IsSeatReservedAsync(request.MovieShowTimeId, seatId);
                    if (seatReserved)
                    {
                        _logger.LogWarning("SeatId {SeatId} is already reserved for MovieShowTimeId {MovieShowTimeId}, UserId {UserId}",
                            seatId, request.MovieShowTimeId, request.UserId);
                        throw new InvalidOperationException($"SeatId {seatId} is already reserved for the selected showtime.");
                    }
                }

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
                return BuildBookingDetailsDto(bookingWithDetails ?? booking, new List<TicketDetailsDto>());
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                if (IsConcurrencyOrDeadlockException(ex))
                {
                    _logger.LogWarning(ex, "Concurrency conflict creating booking for UserId {UserId} - seat(s) may have been taken", request.UserId);
                    throw new InvalidOperationException("One or more selected seats were just booked by another user. Please refresh and try again.");
                }

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
                {
                    _logger.LogWarning("Invalid Argument UserId must be greater than zero");
                    throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
                }

                if (bookingId <= 0)
                {
                    _logger.LogWarning("Invalid Argument BookingId must be greater than zero");
                    throw new ArgumentException("BookingId must be greater than zero.", nameof(bookingId));
                }

                // 2. Get user
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", userId);
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                }

                // 3. Get booking with details
                var booking = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogWarning("BookingId {BookingId} not found", bookingId);
                    throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                }

                // 4. Authorization check
                if (booking.UserId != userId)
                {
                    _logger.LogWarning("Unauthorized: BookingId {BookingId} does not belong to UserId {UserId}", bookingId, userId);
                    throw new UnauthorizedAccessException($"You are not authorized to access booking {bookingId}.");
                }

                var ticketDtos = new List<TicketDetailsDto>();
                if (booking.Tickets != null && booking.Tickets.Any())
                {
                    foreach (var ticket in booking.Tickets)
                    {
                        ticketDtos.Add(_ticketService.MapToDto(ticket));
                    }
                }

                var bookingDetails = BuildBookingDetailsDto(booking, ticketDtos);

                _logger.LogInformation("Successfully retrieved booking {BookingId} for UserId {UserId}", bookingId, userId);
                return bookingDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting BookingId {BookingId} for UserId {UserId}", bookingId, userId);
                throw;
            }
        }

        public async Task<List<BookingListDto>> GetUserBookingsAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting bookings for UserId {UserId}", userId);

                if (userId <= 0)
                {
                    _logger.LogWarning("UserId must be greater than zero. Provided UserId: {UserId}", userId);
                    throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
                }

                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", userId);
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                }

                var bookings = await _unitOfWork.Bookings.GetUserBookingsAsync(userId);

                if (!bookings.Any())
                {
                    _logger.LogInformation("No bookings found for UserId {UserId}", userId);
                    return new List<BookingListDto>();
                }

                var bookingListDtos = bookings.Select(b => new BookingListDto
                {
                    BookingId = b.Id,
                    Status = b.Status,
                    TotalAmount = b.TotalAmount,
                    CreatedAt = b.CreatedAt,
                    MovieShowTimeId = b.MovieShowTime?.Id ?? 0,
                    ShowStartTime = b.MovieShowTime?.ShowStartTime ?? DateTime.MinValue,
                    MovieTitle = b.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                    TicketsCount = b.Tickets?.Count ?? 0
                }).ToList();

                _logger.LogInformation("Retrieved {BookingCount} bookings for UserId {UserId}", bookingListDtos.Count, userId);
                return bookingListDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving bookings for UserId {UserId}", userId);
                throw;
            }
        }

        //Ai
        private BookingDetailsDto BuildBookingDetailsDto(Booking booking, IEnumerable<TicketDetailsDto> tickets)
        {
            var posterUrl = booking.MovieShowTime?.Movie?.MoviePoster ?? string.Empty;

            // Build BookedSeats from BookingSeats
            var bookedSeats = booking.BookingSeats?
                .Select(bs => new SeatDto
                {
                    SeatId = bs.SeatId,
                    SeatLabel = bs.Seat?.SeatLabel ?? string.Empty,
                    SeatRow = bs.Seat?.SeatLabel?.Length > 0 ? bs.Seat.SeatLabel.Substring(0, 1) : string.Empty,
                    SeatColumn = bs.Seat?.SeatLabel?.Length > 1 ? bs.Seat.SeatLabel.Substring(1) : string.Empty
                })
                .ToList() ?? new List<SeatDto>();

            return new BookingDetailsDto
            {
                BookingId = booking.Id,
                Status = booking.Status,
                TotalAmount = booking.TotalAmount,
                CreatedAt = booking.CreatedAt,
                ExpiresAt = booking.ExpiresAt,
                Showtime = new ShowtimeDto
                {
                    MovieShowTimeId = booking.MovieShowTime?.Id ?? 0,
                    MovieTitle = booking.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                    StartTime = booking.MovieShowTime?.ShowStartTime ?? DateTime.MinValue,
                    PosterUrl = posterUrl
                },
                BookedSeats = bookedSeats,
                Tickets = tickets.ToList()
            };
        }
    }
}
