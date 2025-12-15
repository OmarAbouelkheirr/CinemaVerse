using CinemaVerse.Data.Data;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories.Implementations;
using CinemaVerse.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Data.Repositories
{
    public class MovieRepository : Repository<Movie>, IMovieRepository
    {
        private readonly AppDbContext _context;

        public MovieRepository(AppDbContext context, ILogger<Movie> logger)
            : base(context, logger)
        {
            _context = context;
        }

        public async Task<IEnumerable<Movie>> GetByGenreAsync(int genreId)
        {
            try
            {
                _logger.LogInformation("Getting movies by genre. GenreID: {genreId}", genreId);

                var movies = await _context.Movies
                    .Include(m => m.MovieGenres)
                    .Where(m => m.MovieGenres.Any(g => g.GenreID == genreId))
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} movies for GenreId: {GenreId}",
                    movies.Count, genreId);

                return movies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movies by GenreId: {GenreId}", genreId);
                throw;
            }
        }


        public async Task<Movie?> GetMovieWithDetailsByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting movie with details. MovieId: {MovieId}", id);

                var movie = await _context.Movies
                    .Include(m => m.MovieImages)
                    .Include(m => m.MovieShowTimes)
                    .Include(m => m.MovieGenres)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (movie == null)
                {
                    _logger.LogWarning("Movie with id {MovieId} not found", id);
                }

                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting movie with id {MovieId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Movie>> SearchByNameAsync(string keyword)
        {
            try
            {
                _logger.LogInformation("Searching movies with keyword: {Keyword}", keyword);

                keyword = keyword.ToLower();

                var movies = await _context.Movies
                    .Where(m => m.MovieName.ToLower().Contains(keyword))
                    .Include(m => m.MovieGenres)
                    .ToListAsync();

                if (movies.Count == 0)
                {
                    _logger.LogWarning("No movies found matching keyword: {Keyword}", keyword);
                }
                else
                {
                    _logger.LogInformation("Found {Count} movies matching keyword: {Keyword}",
                        movies.Count, keyword);
                }

                return movies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching movies with keyword: {Keyword}", keyword);
                throw;
            }
        }

    }
}
