using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Response;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminSeatService : IAdminSeatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminSeatService> _logger;

        public AdminSeatService(IUnitOfWork unitOfWork, ILogger<AdminSeatService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }


        public async Task<SeatDetailsDto?> GetSeatAsync(int seatId)
        {
            try
            {
                _logger.LogInformation("Getting seat with ID: {SeatId}", seatId);

                if (seatId <= 0)
                {
                    throw new ArgumentException("Seat ID must be a positive integer.", nameof(seatId));
                }
                var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
                if (seat == null)
                    throw new KeyNotFoundException($"Seat with ID {seatId} not found.");

                // Load Hall and Branch details
                var hall = await _unitOfWork.Halls.GetByIdAsync(seat.HallId);
                var branch = hall != null ? await _unitOfWork.Branches.GetByIdAsync(hall.BranchId) : null;

                var dto = new SeatDetailsDto
                {
                    SeatId = seat.Id,
                    SeatLabel = seat.SeatLabel,
                    HallId = seat.HallId,
                    HallNumber = hall?.HallNumber ?? string.Empty,
                    BranchName = branch?.BranchName ?? string.Empty
                };

                _logger.LogInformation("Successfully retrieved seat {SeatId}: {SeatLabel}", seatId, seat.SeatLabel);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seat with ID: {SeatId}", seatId);
                throw;
            }
        }

    }
}
