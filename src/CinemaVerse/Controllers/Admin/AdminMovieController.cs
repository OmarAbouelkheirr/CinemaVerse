using CinemaVerse.Services.DTOs.AdminFlow.AdminMovie.Requests;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.Movie.Flow;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{

    [Route("api/admin/movies")]
    [ApiController]
    public class AdminMovieController : ControllerBase
    {
        private readonly IAdminMovieService _adminMovieService;
        private readonly ILogger<AdminMovieController> _logger;

        public AdminMovieController(
            IAdminMovieService adminMovieService,
            ILogger<AdminMovieController> logger)
        {
            _adminMovieService = adminMovieService;
            _logger = logger;
        }

        /// <summary>
        /// Get all movies with optional filters and pagination
        /// </summary>
        /// <param name="filter">Filter parameters</param>
        /// <returns>Paginated list of movies</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<MovieDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllMovies([FromQuery] AdminMovieFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Admin: Getting all movies with filter: {@Filter}", filter);
                
                var result = await _adminMovieService.GetAllMoviesAsync(filter);
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movies");
                return StatusCode(500, new { error = "An error occurred while retrieving movies" });
            }
        }


        /// <summary>
        /// Get movie details by ID
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <returns>Movie details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MovieDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMovieById(int id)
        {
            try
            {
                _logger.LogInformation("Admin: Getting movie with ID: {MovieId}", id);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "Movie ID must be greater than zero" });
                }

                var movie = await _adminMovieService.GetMovieAsync(id);
                
                if (movie == null)
                {
                    return NotFound(new { error = $"Movie with ID {id} not found" });
                }

                return Ok(movie);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid movie ID: {MovieId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving movie with ID: {MovieId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the movie" });
            }
        }


        /// <summary>
        /// Create a new movie
        /// </summary>
        /// <param name="request">Movie creation request</param>
        /// <returns>Created movie ID</returns>
        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMovie([FromBody] CreateMovieRequestDto request)
        {
            try
            {
                _logger.LogInformation("Admin: Creating new movie: {MovieName}", request?.MovieName);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var movieId = await _adminMovieService.CreateMovieAsync(request!);
                
                _logger.LogInformation("Admin: Successfully created movie with ID: {MovieId}", movieId);
                
                return CreatedAtAction(
                    nameof(GetMovieById),
                    new { id = movieId },
                    new { id = movieId, message = "Movie created successfully" });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null request received");
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request data");
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation");
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating movie");
                return StatusCode(500, new { error = "An error occurred while creating the movie" });
            }
        }

        /// <summary>
        /// Update an existing movie
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <param name="request">Movie update request</param>
        /// <returns>No content on success</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMovie(int id, [FromBody] UpdateMovieRequestDto request)
        {
            try
            {
                _logger.LogInformation("Admin: Updating movie with ID: {MovieId}", id);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "Movie ID must be greater than zero" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _adminMovieService.EditMovieAsync(id, request);
                
                _logger.LogInformation("Admin: Successfully updated movie with ID: {MovieId}", id);
                
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null request received");
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request data for movie ID: {MovieId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Movie not found: {MovieId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating movie with ID: {MovieId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the movie" });
            }
        }

        /// <summary>
        /// Delete a movie
        /// </summary>
        /// <param name="id">Movie ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            try
            {
                _logger.LogInformation("Admin: Deleting movie with ID: {MovieId}", id);
                
                if (id <= 0)
                {
                    return BadRequest(new { error = "Movie ID must be greater than zero" });
                }

                await _adminMovieService.DeleteMovieAsync(id);
                
                _logger.LogInformation("Admin: Successfully deleted movie with ID: {MovieId}", id);
                
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid movie ID: {MovieId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Movie not found: {MovieId}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting movie with ID: {MovieId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the movie" });
            }
        }
    }
}
