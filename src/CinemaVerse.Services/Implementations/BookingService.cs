using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.Booking.Helpers;
using CinemaVerse.Services.DTOs.Booking.Requests;
using CinemaVerse.Services.DTOs.Booking.Responses;
using CinemaVerse.Services.DTOs.Ticket.Response;
using CinemaVerse.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly ILogger<BookingService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public BookingService(ILogger<BookingService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public Task<bool> CancelUserBookingAsync(int userId, int bookingId)
        {
            throw new NotImplementedException();
        }

        public Task<BookingDetailsDto> ConfirmBookingAsync(int userId, int bookingId)
        {
            throw new NotImplementedException();
        }

        public async Task<BookingDetailsDto> CreateBookingAsync(int userId, CreateBookingRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                //Input Validation
                _logger.LogInformation("Creating booking for UserId {UserId}", userId);

                if (request.MovieShowTimeId <= 0)
                {
                    _logger.LogWarning("Invalid Argument MovieShowTimeId {MovieShowTimeId}, UserId {UserId}", request.MovieShowTimeId, userId);
                    throw new ArgumentException("MovieShowTimeId must be a positive integer.", nameof(request.MovieShowTimeId));
                }
                if (request.SeatIds == null || !request.SeatIds.Any())
                {
                    _logger.LogWarning("Invalid Argument SeatIds is null or empty, UserId {UserId}", userId);
                    throw new ArgumentException("SeatIds cannot be null or empty.", nameof(request.SeatIds));
                }

                
                var movieShowTime = await _unitOfWork.MovieShowTimes.GetMovieShowTimeWithDetailsAsync(request.MovieShowTimeId);
                if (movieShowTime == null)
                {
                    _logger.LogWarning("MovieShowTimeId {MovieShowTimeId} not found, UserId {UserId}", request.MovieShowTimeId, userId);
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
                    _logger.LogWarning("MovieShowTimeId {MovieShowTimeId} has already started, UserId {UserId}", request.MovieShowTimeId, userId);
                    throw new InvalidOperationException("Cannot book seats for a showtime that has already started.");
                }

                var hallSeatIds = movieShowTime.Hall.Seats?.Select(s => s.Id).ToList() ?? new List<int>();
                var invalidSeats = request.SeatIds.Where(seatId => !hallSeatIds.Contains(seatId)).ToList();
                if (invalidSeats.Any())
                {
                    _logger.LogWarning("Invalid SeatIds {SeatIds} do not exist in Hall {HallId}, UserId {UserId}", 
                        string.Join(", ", invalidSeats), movieShowTime.Hall.Id, userId);
                    throw new ArgumentException($"SeatIds {string.Join(", ", invalidSeats)} do not exist in this hall.");
                }

                var duplicateSeats = request.SeatIds.GroupBy(s => s).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicateSeats.Any())
                {
                    _logger.LogWarning("Duplicate SeatIds {SeatIds} in request, UserId {UserId}", 
                        string.Join(", ", duplicateSeats), userId);
                    throw new ArgumentException($"Duplicate SeatIds found: {string.Join(", ", duplicateSeats)}");
                }

                // Check seats availability (must be done in transaction to prevent race conditions)
                foreach (var seatId in request.SeatIds)
                {
                    bool seatReserved = await _unitOfWork.MovieShowTimes.IsSeatReservedAsync(request.MovieShowTimeId, seatId);
                    if (seatReserved)
                    {
                        _logger.LogWarning("SeatId {SeatId} is already reserved for MovieShowTimeId {MovieShowTimeId}, UserId {UserId}", 
                            seatId, request.MovieShowTimeId, userId);
                        throw new InvalidOperationException($"SeatId {seatId} is already reserved for the selected showtime.");
                    }
                }

                var totalAmount = (movieShowTime.Price * request.SeatIds.Count) * 1.14m;

                var booking = new Booking
                {
                    UserId = userId,
                    MovieShowTimeId = request.MovieShowTimeId,
                    Status = BookingStatus.Pending,
                    TotalAmount = totalAmount,
                    CreatedAt = DateTime.UtcNow
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
                    booking.Id, userId, request.SeatIds.Count);

                var posterUrl = movieShowTime.Movie.MovieImages?.FirstOrDefault()?.ImageUrl ?? string.Empty;
                return new BookingDetailsDto
                {
                    BookingId = booking.Id,
                    Status = booking.Status,
                    TotalAmount = booking.TotalAmount,
                    CreatedAt = booking.CreatedAt,
                    Showtime = new ShowtimeDto
                    {
                        MovieShowTimeId = movieShowTime.Id,
                        MovieTitle = movieShowTime.Movie.MovieName,
                        StartTime = movieShowTime.ShowStartTime,
                        PosterUrl = posterUrl
                    },
                    Tickets = new List<TicketDetailsDto>() // Empty until payment confirmed and tickets issued
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating booking for UserId {UserId}", userId);
                throw;
            }
        }

        public Task<BookingDetailsDto> GetUserBookingByIdAsync(int userId, int bookingId)
        {

            //await _unitOfWork.BeginTransactionAsync();

            //try
            //{
            //    _logger.LogInformation("Getting BookingId {BookingId} for UserId {UserId}", bookingId, userId);
            //    if (userId <= 0)
            //    {
            //        _logger.LogWarning("Invalid Argument UserId must be greater than zero");
            //        throw new ArgumentException("UserId must be greater than zero.", nameof(userId));
            //    }

            //    //_unitOfWork.

            //    //throw new NotImplementedException();

            //}
            //catch (Exception ex)
            //{
            //    await _unitOfWork.RollbackTransactionAsync(); 
            //    _logger.LogError(ex, "Error getting BookingId {BookingId} for UserId {UserId}", bookingId, userId);
            //    throw;
            //}
            throw new NotImplementedException();
        }

        public Task<List<BookingListDto>> GetUserBookingsAsync(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
