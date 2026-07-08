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
        private new readonly AppDbContext _context;

        public MovieRepository(AppDbContext context, ILogger<Movie> logger)
            : base(context, logger)
        {
            _context = context;
        }

        public async Task<Movie?> GetMovieWithDetailsByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting movie with details. MovieId: {MovieId}", id);

                var movie = await _context.Movies
                    .Include(m => m.CastMembers)
                    .Include(m => m.MovieImages)
                    .Include(m => m.MovieShowTimes)
                        .ThenInclude(ms => ms.Hall)
                            .ThenInclude(h => h.Branch)  // Load nested navigation
                    .Include(m => m.MovieGenres)
                        .ThenInclude(mg => mg.Genre)     // Load Genre entity
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (movie == null)
                {
                    _logger.LogWarning("Movie with id {MovieId} not found", id);
                }

                else
                {
                    _logger.LogInformation("Successfully retrieved movie with {ShowTimeCount} showtimes and {GenreCount} genres", 
                        movie.MovieShowTimes.Count, movie.MovieGenres.Count);
                }

                return movie;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting movie with id {MovieId}", id);
                throw;
            }
        }
    }
}
