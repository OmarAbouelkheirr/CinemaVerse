using CinemaVerse.Services.DTOs.UserFlow.Movie.Flow;
using CinemaVerse.Services.DTOs.Movie.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Movie.Response;

namespace CinemaVerse.Services.Interfaces.User
{
    public interface IMovieService
    {
        Task<BrowseMoviesResponseDto> BrowseMoviesAsync(BrowseMoviesFilterDto browseDto);
        Task<MovieDetailsDto?> GetMovieDetailsAsync(int movieId);
    }
}

