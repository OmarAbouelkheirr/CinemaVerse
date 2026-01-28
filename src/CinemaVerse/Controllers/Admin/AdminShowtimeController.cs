using CinemaVerse.Services.DTOs.AdminFlow.AdminShowtime.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminShowtime.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Route("api/admin/showtimes")]
    [ApiController]
    public class AdminShowtimeController : ControllerBase
    {
        private readonly ILogger<AdminShowtimeController> _logger;
        private readonly IAdminShowtimeService _adminShowtimeService;

        public AdminShowtimeController(IAdminShowtimeService adminShowtimeService, ILogger<AdminShowtimeController> logger)
        {
            _adminShowtimeService = adminShowtimeService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<ShowtimeDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetShowtimes([FromQuery] AdminShowtimeFilterDto adminShowtimeFilterDto)
        {
            try
            {
                _logger.LogInformation("Admin: Getting all showtimes with filter: {@Filter}", adminShowtimeFilterDto);
                var result = await _adminShowtimeService.GetAllShowtimesAsync(adminShowtimeFilterDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving showtimes");
                return StatusCode(500, new { error = "An error occurred while retrieving showtimes" });
            }
        }


        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateShowtime([FromBody] CreateShowtimeRequestDto createShowtimeDto)
        {
            try
            {
                _logger.LogInformation("Admin: Creating a new showtime: {@CreateShowtimeDto}", createShowtimeDto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateShowtimeRequestDto: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                var showtimeId = await _adminShowtimeService.CreateShowtimeAsync(createShowtimeDto);
                _logger.LogInformation("Showtime created successfully with ID: {ShowtimeId}", showtimeId);

                return CreatedAtAction(
                    nameof(GetShowtimeById),
                    new { id = showtimeId },
                    new { id = showtimeId, message = "Showtime created successfully" });
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
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation");
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating showtime");
                return StatusCode(500, new { error = "An error occurred while creating the showtime" });
            }
        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ShowtimeDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetShowtimeById([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin: Getting showtime by ID: {ShowtimeId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid showtime ID: {ShowtimeId}", id);
                    return BadRequest(new { error = "Invalid showtime ID" });
                }
                var showtime = await _adminShowtimeService.GetShowtimeByIdAsync(id);
                if (showtime == null)
                {
                    _logger.LogWarning("Showtime not found: {ShowtimeId}", id);
                    return NotFound(new { error = "Showtime not found" });
                }
                return Ok(showtime);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving showtime by ID: {ShowtimeId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the showtime" });
            }
        }


        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteShowtime([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin: Deleting showtime with ID: {ShowtimeId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid showtime ID: {ShowtimeId}", id);
                    return BadRequest(new { error = "Invalid showtime ID" });
                }
                var deleted = await _adminShowtimeService.DeleteShowtimeAsync(id);
                if (!deleted)
                {
                    _logger.LogWarning("Showtime not found for deletion: {ShowtimeId}", id);
                    return NotFound(new { error = "Showtime not found" });
                }
                _logger.LogInformation("Showtime deleted successfully: {ShowtimeId}", id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting showtime with ID: {ShowtimeId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the showtime" });
            }
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateShowtime([FromRoute] int id, [FromBody] UpdateShowtimeRequestDto updateShowtimeDto)
        {
            try
            {
                _logger.LogInformation("Admin: Updating showtime with ID: {ShowtimeId}", id);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateShowtimeRequestDto: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                if (id <= 0)
                {
                    _logger.LogWarning("Invalid showtime ID: {ShowtimeId}", id);
                    return BadRequest(new { error = "Invalid showtime ID" });
                }

                await _adminShowtimeService.UpdateShowtimeAsync(id, updateShowtimeDto);
                _logger.LogInformation("Showtime with ID {ShowtimeId} updated successfully", id);
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null request received for showtime Id {ShowtimeId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request data for showtime Id {ShowtimeId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Showtime with Id {ShowtimeId} not found", id);
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation for showtime Id {ShowtimeId}", id);
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating showtime with showtime Id {ShowtimeId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the showtime" });
            }
        }
    }

}
