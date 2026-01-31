using CinemaVerse.Extensions;
using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
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
            _logger.LogInformation("Admin: Getting all Halls, Page {Page}, PageSize {PageSize}", filter?.Page ?? 1, filter?.PageSize ?? 20);
            var result = await _adminHallService.GetAllHallsAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(HallDetailsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHallById([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Getting Hall by ID: {HallId}", id);
            var result = await _adminHallService.GetHallWithSeatsByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(HallDetailsResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateHall([FromBody] CreateHallRequestDto createHallDto)
        {
            if (createHallDto == null)
                return BadRequest(new { error = "Request body is required." });

            _logger.LogInformation("Admin: Creating hall for BranchId {BranchId}", createHallDto.BranchId);
            var hallId = await _adminHallService.CreateHallAsync(createHallDto);
            _logger.LogInformation("Hall created successfully with ID: {HallId}", hallId);
            var result = await _adminHallService.GetHallWithSeatsByIdAsync(hallId);
            return CreatedAtAction(nameof(GetHallById), new { id = hallId }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditHall([FromRoute] int id, [FromBody] UpdateHallRequestDto updateHallDto)
        {
            if (updateHallDto == null)
                return BadRequest(new { error = "Request body is required." });

            _logger.LogInformation("Admin: Updating Hall with ID: {HallId}", id);
            await _adminHallService.EditHallAsync(id, updateHallDto);
            _logger.LogInformation("Hall with ID {HallId} updated successfully", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteHall([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Deleting Hall with ID: {HallId}", id);
            await _adminHallService.DeleteHallAsync(id);
            _logger.LogInformation("Hall with ID {HallId} deleted successfully", id);
            return NoContent();
        }
    }
}
