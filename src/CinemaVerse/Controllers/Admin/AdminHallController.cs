using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Route("api/admin/halls")]
    [ApiController]
    public class AdminHallController : ControllerBase
    {
        private readonly ILogger<AdminHallController> _logger;
        private readonly IAdminHallService _adminHallService;

        public AdminHallController(IAdminHallService adminHallService, ILogger<AdminHallController> logger)
        {
            _adminHallService = adminHallService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<HallDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllHalls([FromQuery] AdminHallFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Admin: Getting all Halls with filter: {@Filter}", filter);
                var result = await _adminHallService.GetAllHallsAsync(filter);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Halles");
                return StatusCode(500, new { error = "An error occurred while retrieving Halls" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(HallDetailsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHallById([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin: Getting Hall by ID: {HallId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid Hall ID: {HallId}", id);
                    return BadRequest(new { error = "Invalid Hall ID" });
                }
                var result = await _adminHallService.GetHallWithSeatsByIdAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Hall with ID {HallId} not found", id);
                    return NotFound(new { error = "Hall not found" });
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
                _logger.LogError(ex, "Error retrieving Hall by ID");
                return StatusCode(500, new { error = "An error occurred while retrieving the Hall" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateHall([FromBody] CreateHallRequestDto createHallDto)
        {
            try
            {
                _logger.LogInformation("Admin: Creating a new Hall: {@CreateHallDto}", createHallDto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateHallRequestDto: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                var HallId = await _adminHallService.CreateHallAsync(createHallDto);
                _logger.LogInformation("Hall created successfully with ID: {HallId}", HallId);

                return CreatedAtAction(
                    nameof(GetHallById),
                    new { id = HallId },
                    new { id = HallId, message = "Hall created successfully" });
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
                _logger.LogError(ex, "Error creating Hall");
                return StatusCode(500, new { error = "An error occurred while creating the Hall" });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditHall([FromRoute] int id, [FromBody] UpdateHallRequestDto updateHallDto)
        {
            try
            {
                _logger.LogInformation("Admin: Updating Hall with ID: {HallId}", id);
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateHallRequestDto: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid Hall ID: {HallId}", id);
                    return BadRequest(new { error = "Invalid Hall ID" });
                }

                await _adminHallService.EditHallAsync(id, updateHallDto);
                _logger.LogInformation("Hall with ID {HallId} updated successfully", id);
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null request received for Hall Id {HallId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request data for Hall Id {HallId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Hall with Id {HallId} not found", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Hall with Hall Id {HallId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the Hall" });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteHall([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin: Deleting Hall with ID: {HallId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid Hall ID: {HallId}", id);
                    return BadRequest(new { error = "Invalid Hall ID" });
                }
                await _adminHallService.DeleteHallAsync(id);
                _logger.LogInformation("Hall with ID {HallId} deleted successfully", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Hall with ID {HallId} not found", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Hall with ID {HallId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the Hall" });
            }
        }
    }
}
