using CinemaVerse.Data.Models;

namespace CinemaVerse.Data.Repositories.Interfaces
{
    public interface IMovieRepository : IRepository<Movie>
    {
        Task<Movie?> GetMovieWithDetailsByIdAsync(int id);
    }
}
