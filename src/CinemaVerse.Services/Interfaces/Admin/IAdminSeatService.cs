using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Response;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminSeatService
    {
        Task<int> CreateSeatAsync(CreateSeatRequestDto request);
        Task<List<int>> CreateMultipleSeatsAsync(CreateMultipleSeatsRequestDto request);
        Task<int> UpdateSeatAsync(int seatId, UpdateSeatRequestDto request);
        Task DeleteSeatAsync(int seatId);
        Task<SeatDetailsDto?> GetSeatAsync(int seatId);
        Task<PagedResultDto<SeatDetailsDto>> GetAllSeatsAsync(AdminSeatFilterDto filter);
    }
}
