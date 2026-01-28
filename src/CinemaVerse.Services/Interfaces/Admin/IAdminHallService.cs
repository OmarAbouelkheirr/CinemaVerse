using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Response;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminHallService
    {
        public Task<int> CreateHallAsync(CreateHallRequestDto Request);
        public Task<int> EditHallAsync(int HallId, UpdateHallRequestDto Request);
        public Task DeleteHallAsync(int HallId);
        public Task<PagedResultDto<HallDetailsResponseDto>> GetAllHallsAsync(AdminHallFilterDto Filter);
        public Task<HallDetailsResponseDto?> GetHallWithSeatsByIdAsync(int HallId);
    }
}
