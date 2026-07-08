using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Booking.Helpers;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using CinemaVerse.Services.Mappers;
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


        public async Task<int> CreateBookingAsync(CreateBookingRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating booking for user {UserId}", request.UserId);
                if (request.SeatIds == null || !request.SeatIds.Any())
                {
                    throw new ArgumentException("At least one seat must be selected for booking.");
                }
                if (request.UserId <= 0)
                    throw new ArgumentException("User ID must be a positive integer.", nameof(request.UserId));
                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {request.UserId} not found.");
                if (request.MovieShowTimeId <= 0)
                    throw new ArgumentException("Movie showtime ID must be a positive integer.", nameof(request.MovieShowTimeId));
                var movieShowTime = await _unitOfWork.MovieShowTimes.GetByIdAsync(request.MovieShowTimeId);
                if (movieShowTime == null)
                    throw new KeyNotFoundException($"MovieShowTime with ID {request.MovieShowTimeId} not found.");
                if (movieShowTime.ShowStartTime <= DateTime.UtcNow)
                    throw new InvalidOperationException("Cannot book seats for a show that has already started or passed.");
                var requestedSeats = await _unitOfWork.Seats.FindAllAsync(s => request.SeatIds.Contains(s.Id));
                if (requestedSeats.Count() != request.SeatIds.Count)
                {
                    var foundSeatIds = requestedSeats.Select(s => s.Id).ToList();
                    var missingSeatIds = request.SeatIds.Except(foundSeatIds).ToList();

                    throw new ArgumentException($"The following seat IDs were not found: {string.Join(", ", missingSeatIds)}");
                }
                var invalidHallSeats = requestedSeats.Where(s => s.HallId != movieShowTime.HallId).ToList();
                if (invalidHallSeats.Any())
                {
                    var invalidSeatIds = invalidHallSeats.Select(s => s.Id).ToList();
                    throw new ArgumentException($"The following seats do not belong to hall {movieShowTime.HallId}: {string.Join(", ", invalidSeatIds)}");
                }
                var existingBookings = await _unitOfWork.BookingSeat
                        .FindAllAsync(bs =>
                            request.SeatIds.Contains(bs.SeatId) &&
                            bs.Booking.MovieShowTimeId == request.MovieShowTimeId &&
                            (bs.Booking.Status == BookingStatus.Confirmed ||
                             bs.Booking.Status == BookingStatus.Pending));

                if (existingBookings.Any())
                {
                    var bookedSeatIds = existingBookings.Select(bs => bs.SeatId).Distinct().ToList();
                    throw new InvalidOperationException($"The following seats are already booked: {string.Join(", ", bookedSeatIds)}");
                }
                decimal totalAmount = requestedSeats.Count() * movieShowTime.Price;
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
                await _unitOfWork.SaveChangesAsync();

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

                _logger.LogInformation("Successfully created booking {BookingId} for user {UserId} with {SeatCount} seats",
                    booking.Id, request.UserId, request.SeatIds.Count);

                return booking.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating booking for user {UserId}", request.UserId);
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
                    throw new ArgumentException("Booking ID must be a positive integer.", nameof(bookingId));
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
                if (booking == null)
                    throw new KeyNotFoundException("Booking not found.");
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

        public async Task<int> UpdateBookingStatusAsync(int bookingId, BookingStatus newStatus)
        {
            try
            {
                _logger.LogInformation("Updating status for booking {BookingId} to {newStatus}", bookingId, newStatus);
                if (bookingId <= 0)
                {
                    throw new ArgumentException("Booking ID must be a positive integer.", nameof(bookingId));
                }
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
                if (booking == null)
                    throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                await _unitOfWork.Bookings.UpdateBookingStatusAsync(bookingId, newStatus);
                var result = await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully updated status for booking {BookingId} to {newStatus}", bookingId, newStatus);
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
                    throw new ArgumentNullException(nameof(filter), "AdminBookingFilterDto cannot be null.");

                // Validate pagination
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > PaginationConstants.MaxPageSize)
                    filter.PageSize = PaginationConstants.DefaultPageSize;

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
                var bookingDtos = bookings.Select(booking => BookingMapper.ToDetailsDto(booking)).ToList();

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

        public async Task<BookingDetailsDto> GetBookingByIdAsync(int bookingId)
        {
            try
            {
                _logger.LogInformation("Getting booking with Id {BookingId}", bookingId);
                if (bookingId <= 0)
                {
                    throw new ArgumentException("Booking ID must be a positive integer.", nameof(bookingId));
                }
                var bookingWithDetails = await _unitOfWork.Bookings.GetBookingWithDetailsAsync(bookingId);
                if (bookingWithDetails == null)
                    throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                return BookingMapper.ToDetailsDto(bookingWithDetails);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting booking with Id {BookingId}", bookingId);
                throw;
            }
        }
    }
}
