using CinemaVerse.Data.Models;

namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface IMovieRepository : IRepository<Movie>
    {
        Task<Movie?> GetMovieWithDetailsByIdAsync(int id);
        Task<IEnumerable<Movie>> GetByGenreAsync(int genreId);
        Task<IEnumerable<Movie>> SearchByNameAsync(string keyword);
    }
}
