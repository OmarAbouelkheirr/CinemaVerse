using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.AdminFlow.AdminGenre.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminGenre.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Route("api/admin/genres")]
    [ApiController]
    public class AdminGenreController : ControllerBase
    {
        private readonly ILogger<AdminGenreController> _logger;
        private readonly IAdminGenreService _adminGenreService;
        public AdminGenreController(IAdminGenreService adminGenreService, ILogger<AdminGenreController> logger)
        {
            _adminGenreService = adminGenreService;
            _logger = logger;
        }
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<GenreDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllGenres([FromQuery] AdminGenreFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Admin: Getting all genres with filter: {@Filter}", filter);
                var result = await _adminGenreService.GetAllGenresAsync(filter);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving genres");
                return StatusCode(500, new { error = "An error occurred while retrieving genres" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GenreDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGenreById([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin: Getting genre by ID: {GenreId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid genre ID: {GenreId}", id);
                    return BadRequest(new { error = "Invalid genre ID" });
                }
                var result = await _adminGenreService.GetGenreAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Genre with ID {GenreId} not found", id);
                    return NotFound(new { error = "Genre not found" });
                }
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving genre by ID");
                return StatusCode(500, new { error = "An error occurred while retrieving the genre" });
            }
        }


        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateGenre([FromBody] CreateGenreRequestDto createGenreDto)
        {
            try
            {
                _logger.LogInformation("Admin: Creating a new genre: {@CreateGenreDto}", createGenreDto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateGenreRequestDto: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                var GenreId = await _adminGenreService.CreateGenreAsync(createGenreDto);
                _logger.LogInformation("Genre created successfully with ID: {GenreId}", GenreId);

                return CreatedAtAction(
                    nameof(GetGenreById),
                    new { id = GenreId },
                    new { id = GenreId, message = "Genre created successfully" });
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
                _logger.LogError(ex, "Error creating genre");
                return StatusCode(500, new { error = "An error occurred while creating the genre" });
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditGenre([FromRoute] int id, [FromBody] UpdateGenreRequestDto updateGenreDto)
        {
            try
            {
                _logger.LogInformation("Admin: Updating genre with ID: {GenreId}", id);
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateGenreRequestDto: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid genre ID: {GenreId}", id);
                    return BadRequest(new { error = "Invalid genre ID" });
                }

                await _adminGenreService.UpdateGenreAsync(id, updateGenreDto);
                _logger.LogInformation("Genre with ID {GenreId} updated successfully", id);
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null request received for genre Id {GenreId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request data for genre Id {GenreId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Genre with Id {GenreId} not found", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating genre with genre Id {GenreId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the genre" });
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteGenre([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin: Deleting genre with ID: {GenreId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid genre ID: {GenreId}", id);
                    return BadRequest(new { error = "Invalid genre ID" });
                }
                await _adminGenreService.DeleteGenreAsync(id);
                _logger.LogInformation("Genre with ID {GenreId} deleted successfully", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Genre with ID {GenreId} not found", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting genre with ID {GenreId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the genre" });
            }
        }
    }
}
