using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.Movie.Flow;
using CinemaVerse.Services.DTOs.Movie.Requests;
using CinemaVerse.Services.DTOs.Movie.Response;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.User
{
    [Route("api/movies")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly ILogger<MovieController> _logger;
        private readonly IMovieService _movieService;
        public MovieController(ILogger<MovieController> logger, IMovieService movieService)
        {
            _logger = logger;
            _movieService = movieService;
        }
        [HttpGet]
        [ProducesResponseType(typeof(BrowseMoviesResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BrowseMovie([FromQuery] BrowseMoviesFilterDto filter)
        {
            try
            {
                if (filter == null)
                {
                    _logger.LogWarning("User: BrowseMovies called with null filter");
                    return BadRequest(new { error = "Request cannot be null" });
                }

                _logger.LogInformation("User: Browsing movies with filter: {@filter}", filter);

                var result = await _movieService.BrowseMoviesAsync(filter);

                _logger.LogInformation("User: Successfully retrieved movies with filter: {@filter}", filter);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing movies");
                return StatusCode(500, new { error = "An error occurred while browsing movies" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MovieDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMovieDetails([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("User: Getting movie details for ID: {MovieId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid movie ID: {MovieId}", id);
                    return BadRequest(new { error = "Invalid movie ID" });
                }
                var result = await _movieService.GetMovieDetailsAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Movie not found for ID: {MovieId}", id);
                    return NotFound(new { error = "Movie not found" });
                }
                _logger.LogInformation("User: Successfully retrieved movie details for ID: {MovieId}", id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movie details");
                return StatusCode(500, new { error = "An error occurred while retrieving movie details" });
            }
        }
    }
}
