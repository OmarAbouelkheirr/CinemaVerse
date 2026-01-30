using CinemaVerse.Extensions;
using CinemaVerse.Services.DTOs.AdminFlow.AdminMovie.Requests;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.UserFlow.Movie.Flow;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
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

        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<MovieDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllMovies([FromQuery] AdminMovieFilterDto filter)
        {
            _logger.LogInformation("Admin: Getting all movies with filter: {@Filter}", filter);
            var result = await _adminMovieService.GetAllMoviesAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MovieDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMovieById(int id)
        {
            _logger.LogInformation("Admin: Getting movie with ID: {MovieId}", id);
            var movie = await _adminMovieService.GetMovieAsync(id);
            if (movie == null)
                return NotFound(new { error = $"Movie with ID {id} not found" });
            return Ok(movie);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateMovie([FromBody] CreateMovieRequestDto request)
        {
            _logger.LogInformation("Admin: Creating new movie: {MovieName}", request?.MovieName);
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);
            var movieId = await _adminMovieService.CreateMovieAsync(request!);
            _logger.LogInformation("Admin: Successfully created movie with ID: {MovieId}", movieId);
            var movie = await _adminMovieService.GetMovieAsync(movieId);
            return CreatedAtAction(nameof(GetMovieById), new { id = movieId }, movie);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMovie(int id, [FromBody] UpdateMovieRequestDto request)
        {
            _logger.LogInformation("Admin: Updating movie with ID: {MovieId}", id);
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);
            await _adminMovieService.EditMovieAsync(id, request);
            _logger.LogInformation("Admin: Successfully updated movie with ID: {MovieId}", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            _logger.LogInformation("Admin: Deleting movie with ID: {MovieId}", id);
            await _adminMovieService.DeleteMovieAsync(id);
            _logger.LogInformation("Admin: Successfully deleted movie with ID: {MovieId}", id);
            return NoContent();
        }
    }
}
