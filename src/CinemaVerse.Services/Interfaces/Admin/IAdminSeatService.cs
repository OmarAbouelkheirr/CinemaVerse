using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Response;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminSeatService
    {

        Task<SeatDetailsDto?> GetSeatAsync(int seatId);
    }
}
