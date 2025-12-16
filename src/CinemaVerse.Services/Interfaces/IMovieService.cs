using CinemaVerse.Services.DTOs.Movie.Flow;
using CinemaVerse.Services.DTOs.Movie.Requests;
using CinemaVerse.Services.DTOs.Movie.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CinemaVerse.Services.Interfaces
{
    public interface IMovieService
    {
        Task<BrowseMoviesResponseDto> BrowseMoviesAsync(BrowseMoviesRequestDto browseDto);
        Task<BrowseMoviesResponseDto> SearchMoviesAsync(BrowseMoviesRequestDto browseDto);
        Task<MovieDetailsDto?> GetMovieDetailsAsync(int movieId);
        Task<IEnumerable<MovieShowTimeDto>> GetMovieShowTimesAsync(int movieId);
    }
}

