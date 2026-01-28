using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Response;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminSeatService
    {

        Task<SeatDetailsDto?> GetSeatAsync(int seatId);
        Task<PagedResultDto<SeatDetailsDto>> GetAllSeatsAsync(AdminSeatFilterDto filter);
    }
}
