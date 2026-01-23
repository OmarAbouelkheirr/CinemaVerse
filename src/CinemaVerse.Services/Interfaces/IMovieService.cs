using CinemaVerse.Services.DTOs.Movie.Flow;
using CinemaVerse.Services.DTOs.Movie.Requests;
using CinemaVerse.Services.DTOs.Movie.Response;

namespace CinemaVerse.Services.Interfaces
{
    public interface IMovieService
    {
        Task<BrowseMoviesResponseDto> BrowseMoviesAsync(BrowseMoviesRequestDto browseDto);
        Task<MovieDetailsDto?> GetMovieDetailsAsync(int movieId);
    }
}

