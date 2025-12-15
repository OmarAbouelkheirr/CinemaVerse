using CinemaVerse.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.Interfaces
{
    public interface IMovieService
    {
        Task<IEnumerable<Movie>> BrowseMoviesAsync(int? GenreId = null, string? Section = null, DateOnly? Date = null,int Page = 1,int PageSize = 20);
        Task<IEnumerable<Movie>> SearchMoviesAsync(string Query,int? GenreId = null, string? Section = null, DateOnly? Date = null, int Page = 1, int PageSize = 20);
        Task<Movie?> GetMovieDetailsAsync(int MovieId);
        Task<IEnumerable<MovieShowTime>> GetMovieShowTimesAsync(int MovieId, DateOnly Date);
    }
}

