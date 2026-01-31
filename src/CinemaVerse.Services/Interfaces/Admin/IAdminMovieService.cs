using CinemaVerse.Services.DTOs.AdminFlow.AdminMovie.Requests;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.UserFlow.Movie.Flow;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminMovieService
    {
        Task<int> CreateMovieAsync(CreateMovieRequestDto request);
        Task<int> EditMovieAsync(int movieId, UpdateMovieRequestDto request);
        Task DeleteMovieAsync(int movieId);

        Task<MovieDetailsDto> GetMovieAsync(int movieId);
        Task<PagedResultDto<MovieDetailsDto>> GetAllMoviesAsync(AdminMovieFilterDto filter);
    }
}
