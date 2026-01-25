using CinemaVerse.Services.DTOs.AdminFlow.AdminGenre.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminGenre.Response;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminGenreService
    {
        Task<int> CreateGenreAsync(CreateGenreRequestDto request);
        Task<int> UpdateGenreAsync(int genreId, UpdateGenreRequestDto request);
        Task DeleteGenreAsync(int genreId);
        Task<GenreDetailsDto?> GetGenreAsync(int genreId);
        Task<PagedResultDto<GenreDetailsDto>> GetAllGenresAsync(AdminGenreFilterDto filter);
    }
}
