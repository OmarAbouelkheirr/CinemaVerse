using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.HallSeat.Responses;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.User
{
    public class HallSeatService : IHallSeatService
    {
        private readonly ILogger<HallSeatService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HallSeatService(ILogger<HallSeatService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<HallWithSeatsDto> GetHallWithSeatsAsync(int MovieShowTimeId)
        {
            try
            {
                _logger.LogInformation("Fetching hall with seats for MovieShowTimeId: {MovieShowTimeId}", MovieShowTimeId);
                if (MovieShowTimeId <= 0)
                {
                    _logger.LogWarning("Invalid MovieShowTimeId {MovieShowTimeId}", MovieShowTimeId);
                    throw new ArgumentException("MovieShowTimeId must be a positive integer.", nameof(MovieShowTimeId));
                }

                var MovieShowTime = await _unitOfWork.MovieShowTimes.GetMovieShowTimeWithDetailsAsync(MovieShowTimeId);
                _logger.LogInformation("Fetched hall details for MovieShowTimeId: {MovieShowTimeId}", MovieShowTimeId);

                if (MovieShowTime == null)
                {
                    _logger.LogWarning("MovieShowTime with ID {MovieShowTimeId} not found", MovieShowTimeId);
                    throw new KeyNotFoundException($"MovieShowTime with ID {MovieShowTimeId} not found.");
                }

                if (MovieShowTime.Hall == null)
                {
                    _logger.LogError("Hall not loaded for MovieShowTimeId: {MovieShowTimeId}", MovieShowTimeId);
                    throw new InvalidOperationException($"Hall information is missing for MovieShowTime {MovieShowTimeId}.");
                }

                var availableSeats = await _unitOfWork.MovieShowTimes.GetAvailableSeatsAsync(MovieShowTimeId);
                var reservedSeats = await _unitOfWork.MovieShowTimes.GetReservedSeatsAsync(MovieShowTimeId);

                var availableSeatsList = availableSeats.ToList();
                var reservedSeatsList = reservedSeats.ToList();

                _logger.LogInformation(
                    "Successfully fetched hall details for MovieShowTimeId: {MovieShowTimeId}. Available: {AvailableCount}, Reserved: {ReservedCount}, Capacity: {Capacity}",
                    MovieShowTimeId, availableSeatsList.Count, reservedSeatsList.Count, MovieShowTime.Hall.Capacity);

                return new HallWithSeatsDto
                {
                    HallId = MovieShowTime.Hall.Id,
                    HallNumber = MovieShowTime.Hall.HallNumber,
                    Branch = MovieShowTime.Hall.Branch.BranchLocation,
                    Capacity = MovieShowTime.Hall.Capacity,
                    HallType = MovieShowTime.Hall.HallType,
                    AvailableSeats = availableSeatsList.Select(s => new SeatDto
                    {
                        SeatId = s.Id,
                        SeatLabel = s.SeatLabel,
                        SeatRow = s.SeatLabel.Substring(0, 1),
                        SeatColumn = s.SeatLabel.Substring(1)
                    }).ToList()
                    ,
                    ReservedSeats = reservedSeats.Select(s => new SeatDto
                    {
                        SeatId = s.Id,
                        SeatLabel = s.SeatLabel,
                        SeatRow = s.SeatLabel.Substring(0, 1),
                        SeatColumn = s.SeatLabel.Substring(1)
                    }).ToList()
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Fetching hall with seats for MovieShowTimeId: {MovieShowTimeId}", MovieShowTimeId);
                throw;
            }

        }

    }
}
