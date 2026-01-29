using CinemaVerse.Extensions;
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
            _logger.LogInformation("Admin: Getting all showtimes with filter: {@Filter}", adminShowtimeFilterDto);
            var result = await _adminShowtimeService.GetAllShowtimesAsync(adminShowtimeFilterDto);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShowtimeDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateShowtime([FromBody] CreateShowtimeRequestDto createShowtimeDto)
        {
            _logger.LogInformation("Admin: Creating a new showtime: {@CreateShowtimeDto}", createShowtimeDto);
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);
            var showtimeId = await _adminShowtimeService.CreateShowtimeAsync(createShowtimeDto);
            _logger.LogInformation("Showtime created successfully with ID: {ShowtimeId}", showtimeId);
            var showtime = await _adminShowtimeService.GetShowtimeByIdAsync(showtimeId);
            return CreatedAtAction(nameof(GetShowtimeById), new { id = showtimeId }, showtime);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ShowtimeDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetShowtimeById([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Getting showtime by ID: {ShowtimeId}", id);
            var showtime = await _adminShowtimeService.GetShowtimeByIdAsync(id);
            if (showtime == null)
                return NotFound(new { error = "Showtime not found" });
            return Ok(showtime);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateShowtime([FromRoute] int id, [FromBody] UpdateShowtimeRequestDto updateShowtimeDto)
        {
            _logger.LogInformation("Admin: Updating showtime with ID: {ShowtimeId}", id);
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);
            await _adminShowtimeService.UpdateShowtimeAsync(id, updateShowtimeDto);
            _logger.LogInformation("Showtime with ID {ShowtimeId} updated successfully", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteShowtime([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Deleting showtime with ID: {ShowtimeId}", id);
            var deleted = await _adminShowtimeService.DeleteShowtimeAsync(id);
            if (!deleted)
                return NotFound(new { error = "Showtime not found" });
            _logger.LogInformation("Showtime deleted successfully: {ShowtimeId}", id);
            return NoContent();
        }
    }
}
