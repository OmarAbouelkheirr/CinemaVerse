using CinemaVerse.Services.DTOs.HallSeat.Responses;

namespace CinemaVerse.Services.Interfaces.User
{
    public interface IHallSeatService
    {
        Task<HallWithSeatsDto> GetHallWithSeatsAsync(int MovieShowTimeId);
    }
}
