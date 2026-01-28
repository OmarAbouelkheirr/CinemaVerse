using CinemaVerse.Services.DTOs.AdminFlow.AdminShowtime.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminShowtime.Response;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminShowtimeService
    {
        Task<int> CreateShowtimeAsync(CreateShowtimeRequestDto request);
        Task<int> UpdateShowtimeAsync(int showtimeId, UpdateShowtimeRequestDto request);
        Task<bool> DeleteShowtimeAsync(int showtimeId);
        Task<ShowtimeDetailsDto?> GetShowtimeByIdAsync(int showtimeId);
        Task<PagedResultDto<ShowtimeDetailsDto>> GetAllShowtimesAsync(AdminShowtimeFilterDto filter);
    }
}
