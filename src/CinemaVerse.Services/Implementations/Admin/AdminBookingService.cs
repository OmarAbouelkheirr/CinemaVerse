using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests;
using CinemaVerse.Services.DTOs.Booking.Helpers;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.HallSeat.Responses;
using CinemaVerse.Services.DTOs.Ticket.Response;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminBookingService : IAdminBookingService
    {
        private readonly ILogger<AdminBookingService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public AdminBookingService(ILogger<AdminBookingService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        public async Task<int> CreateBookingAsync(CreateBookingRequestDto Request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating booking for user {UserId}", Request.UserId);
                if (Request.SeatIds == null || !Request.SeatIds.Any())
                {
                    _logger.LogWarning("No seats provided for booking by user {UserId}", Request.UserId);
                    throw new ArgumentException("At least one seat must be selected for booking.");
                }
                if (Request.UserId <= 0)
                {
                    _logger.LogWarning("Invalid UserId {UserId} provided for booking", Request.UserId);
                    throw new ArgumentException("A valid UserId must be provided for booking.");
                }
                var User = await _unitOfWork.Users.GetByIdAsync(Request.UserId);
                if (User == null)//apply roles
                {
                    _logger.LogWarning("User {UserId} not found or is not a customer", Request.UserId);
                    throw new ArgumentException("A valid customer UserId must be provided for booking.");
                }
                if (Request.MovieShowTimeId <= 0)
                {
                    _logger.LogWarning("Invalid MovieShowTimeId {MovieShowTimeId} provided for booking", Request.MovieShowTimeId);
                    throw new ArgumentException("A valid MovieShowTimeId must be provided for booking.");
                }
                var MovieShowTime = await _unitOfWork.MovieShowTimes.GetByIdAsync(Request.MovieShowTimeId);
                if (MovieShowTime == null)
                {
                    _logger.LogWarning("MovieShowTime {MovieShowTimeId} not found", Request.MovieShowTimeId);
                    throw new ArgumentException("A valid MovieShowTimeId must be provided for booking.");
                }
                if (MovieShowTime.ShowStartTime <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Cannot book seats for past or ongoing show {MovieShowTimeId}", Request.MovieShowTimeId);
                    throw new InvalidOperationException("Cannot book seats for a show that has already started or passed.");
                }
                var requestedSeats = await _unitOfWork.Seats.FindAllAsync(s => Request.SeatIds.Contains(s.Id));
                if (requestedSeats.Count() != Request.SeatIds.Count)
                {
                    var foundSeatIds = requestedSeats.Select(s => s.Id).ToList();
                    var missingSeatIds = Request.SeatIds.Except(foundSeatIds).ToList();

                    _logger.LogWarning("Seats not found: {MissingSeatIds}", string.Join(", ", missingSeatIds));
                    throw new ArgumentException($"The following seat IDs were not found: {string.Join(", ", missingSeatIds)}");
                }
                var invalidHallSeats = requestedSeats.Where(s => s.HallId != MovieShowTime.HallId).ToList();
                if (invalidHallSeats.Any())
                {
                    var invalidSeatIds = invalidHallSeats.Select(s => s.Id).ToList();
                    _logger.LogWarning("Seats do not belong to the correct hall: {InvalidSeatIds}", string.Join(", ", invalidSeatIds));
                    throw new ArgumentException($"The following seats do not belong to hall {MovieShowTime.HallId}: {string.Join(", ", invalidSeatIds)}");
                }
                var existingBookings = await _unitOfWork.BookingSeat
                        .FindAllAsync(bs =>
                            Request.SeatIds.Contains(bs.SeatId) &&
                            bs.Booking.MovieShowTimeId == Request.MovieShowTimeId &&
                            (bs.Booking.Status == BookingStatus.Confirmed ||
                             bs.Booking.Status == BookingStatus.Pending));

                if (existingBookings.Any())
                {
                    var bookedSeatIds = existingBookings.Select(bs => bs.SeatId).Distinct().ToList();
                    _logger.LogWarning("Seats already booked for this showtime: {BookedSeatIds}", string.Join(", ", bookedSeatIds));
                    throw new InvalidOperationException($"The following seats are already booked: {string.Join(", ", bookedSeatIds)}");
                }
                decimal totalAmount = requestedSeats.Count() * MovieShowTime.Price;
                var booking = new Booking
                {
                    UserId = Request.UserId,
                    MovieShowTimeId = Request.MovieShowTimeId,
                    Status = BookingStatus.Pending,
                    TotalAmount = totalAmount,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15)
                };
                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                foreach (var seatId in Request.SeatIds)
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

                _logger.LogInformation("Successfully created booking {BookingId} for user {UserId} with {SeatCount} seats",
                    booking.Id, Request.UserId, Request.SeatIds.Count);

                return booking.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating booking for user {UserId}", Request.UserId);
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task DeleteBookingAsync(int bookingId)
        {
            try
            {
                _logger.LogInformation("Deleting booking {BookingId}", bookingId);
                if (bookingId <= 0)
                {
                    _logger.LogWarning("Invalid BookingId {BookingId} provided for deletion", bookingId);
                    throw new ArgumentException("A valid BookingId must be provided for deletion.");
                }
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking {BookingId} not found for deletion", bookingId);
                    throw new ArgumentException("Booking not found.");
                }
                await _unitOfWork.Bookings.DeleteAsync(booking);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted booking {BookingId}", bookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting booking {BookingId}", bookingId);
                throw;
            }
        }

        public async Task<int> UpdateBookingStatusAsync(int bookingId, BookingStatus NewStatus)
        {
            try
            {
                _logger.LogInformation("Updating status for booking {BookingId} to {NewStatus}", bookingId, NewStatus);
                if (bookingId <= 0)
                {
                    _logger.LogWarning("Invalid BookingId {BookingId} provided", bookingId);
                    throw new ArgumentException("A valid BookingId must be provided.", nameof(bookingId));
                }

                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking {BookingId} not found", bookingId);
                    throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                }
                await _unitOfWork.Bookings.UpdateBookingStatusAsync(bookingId, NewStatus);
                var result = await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully updated status for booking {BookingId} to {NewStatus}", bookingId, NewStatus);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating booking status for booking {BookingId}", bookingId);
                throw;
            }
        }

        public async Task<PagedResultDto<BookingDetailsDto>> GetAllBookingsAsync(AdminBookingFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting all bookings with filter: {@Filter}", filter);

                if (filter == null)
                {
                    _logger.LogWarning("AdminBookingFilterDto is null.");
                    throw new ArgumentNullException(nameof(filter), "AdminBookingFilterDto cannot be null.");
                }

                // Validate pagination
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > 100)
                    filter.PageSize = 20;

                // Build query
                var query = _unitOfWork.Bookings.GetQueryable();

                // Apply filters
                if (filter.UserId.HasValue)
                {
                    query = query.Where(b => b.UserId == filter.UserId.Value);
                }

                if (filter.MovieShowTimeId.HasValue)
                {
                    query = query.Where(b => b.MovieShowTimeId == filter.MovieShowTimeId.Value);
                }

                if (filter.Status.HasValue)
                {
                    query = query.Where(b => b.Status == filter.Status.Value);
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

                // Search by user email or movie name
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(b =>
                        b.User.Email.ToLower().Contains(searchLower) ||
                        b.MovieShowTime.Movie.MovieName.ToLower().Contains(searchLower));
                }

                // Get total count before pagination
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
                else if (sortBy == "expiresat")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.ExpiresAt)
                        : query.OrderByDescending(b => b.ExpiresAt);
                }
                else // Default
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
                var bookingDtos = bookings.Select(booking => BuildBookingDetailsDto(booking,
                    booking.Tickets.Select(t => new TicketDetailsDto
                    {
                        TicketId = t.Id,
                        SeatLabel = t.Seat?.SeatLabel ?? string.Empty,
                        Price = t.Price,
                        QrToken = t.QrToken
                    })
                )).ToList();

                var pagedResult = new PagedResultDto<BookingDetailsDto>
                {
                    Items = bookingDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };

                _logger.LogInformation("Retrieved {Count} bookings out of {Total} total",
                    bookingDtos.Count, totalCount);

                return pagedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all bookings.");
                throw;
            }
        }

        public async Task<BookingDetailsDto?> GetBookingByIdAsync(int bookingId)
        {
            try
            {
                _logger.LogInformation("Getting booking with Id {BookingId}", bookingId);
                if (bookingId <= 0)
                {
                    _logger.LogWarning("Invalid BookingId {BookingId} provided for retrieval", bookingId);
                    throw new ArgumentException("A valid BookingId must be provided.");
                }
                var bookingWithDetails = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);
                if (bookingWithDetails == null)
                {
                    _logger.LogWarning("Booking {BookingId} not found", bookingId);
                    return null;
                }
                var tickets = bookingWithDetails.Tickets?.Select(t => new TicketDetailsDto
                {
                    TicketId = t.Id,
                    SeatLabel = t.Seat?.SeatLabel ?? string.Empty,
                    Price = t.Price,
                    QrToken = t.QrToken
                }) ?? Enumerable.Empty<TicketDetailsDto>();
                return BuildBookingDetailsDto(bookingWithDetails, tickets);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting booking with Id {BookingId}", bookingId);
                throw;
            }
        }
        private BookingDetailsDto BuildBookingDetailsDto(Booking booking, IEnumerable<TicketDetailsDto> tickets)
        {
            // Get poster URL (safely handle null)
            var posterUrl = booking.MovieShowTime?.Movie?.MovieImages?.FirstOrDefault()?.ImageUrl ?? string.Empty;

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
