using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Response;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminHallService
    {
        public Task<int> CreateHallAsync(CreateHallRequestDto request);
        public Task<int> EditHallAsync(int hallId, UpdateHallRequestDto request);
        public Task DeleteHallAsync(int hallId);
        public Task<PagedResultDto<HallDetailsResponseDto>> GetAllHallsAsync(AdminHallFilterDto filter);
        public Task<HallDetailsResponseDto> GetHallWithSeatsByIdAsync(int hallId);
    }
}
