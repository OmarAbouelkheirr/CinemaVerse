using CinemaVerse.Extensions;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/branches")]
    [ApiController]
    public class AdminBranchController : ControllerBase
    {
        private readonly ILogger<AdminBranchController> _logger;
        private readonly IAdminBranchService _adminBranchService;
        private readonly IAdminHallService _adminHallService;

        public AdminBranchController(IAdminBranchService adminBranchService, IAdminHallService adminHallService, ILogger<AdminBranchController> logger)
        {
            _adminBranchService = adminBranchService;
            _adminHallService = adminHallService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<BranchDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBranches([FromQuery] AdminBranchFilterDto filter)
        {
            if (filter == null)
                filter = new AdminBranchFilterDto();
            _logger.LogInformation("Admin: Getting all branches, Page {Page}, PageSize {PageSize}", filter.Page, filter.PageSize);
            var result = await _adminBranchService.GetAllBranchesAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BranchDetailsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBranchById([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Getting branch by ID: {BranchId}", id);
            var result = await _adminBranchService.GetBranchByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("{id}/halls")]
        [ProducesResponseType(typeof(PagedResultDto<CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Response.HallDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHallsByBranchId([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Getting halls for branch ID: {BranchId}", id);
            var filter = new CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Requests.AdminHallFilterDto { BranchId = id, PageSize = int.MaxValue };
            var result = await _adminHallService.GetAllHallsAsync(filter);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BranchDetailsResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequestDto createBranchDto)
        {
            if (createBranchDto == null)
                return BadRequest(new { error = new CinemaVerse.Models.ErrorResponse { Message = "Request body is required.", Code = "VALIDATION_ERROR" } });

            _logger.LogInformation("Admin: Creating new branch");
            var branchId = await _adminBranchService.CreateBranchAsync(createBranchDto);
            _logger.LogInformation("Branch created successfully with ID: {BranchId}", branchId);
            var result = await _adminBranchService.GetBranchByIdAsync(branchId);
            return CreatedAtAction(nameof(GetBranchById), new { id = branchId }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditBranch([FromRoute] int id, [FromBody] UpdateBranchRequestDto updateBranchDto)
        {
            if (updateBranchDto == null)
                return BadRequest(new { error = new CinemaVerse.Models.ErrorResponse { Message = "Request body is required.", Code = "VALIDATION_ERROR" } });

            _logger.LogInformation("Admin: Updating branch with ID: {BranchId}", id);
            await _adminBranchService.EditBranchAsync(id, updateBranchDto);
            _logger.LogInformation("Branch with ID {BranchId} updated successfully", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBranch([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Deleting branch with ID: {BranchId}", id);
            await _adminBranchService.DeleteBranchAsync(id);
            _logger.LogInformation("Branch with ID {BranchId} deleted successfully", id);
            return NoContent();
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(BranchSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBranchSummary()
        {
            _logger.LogInformation("Admin: Getting branch summary stats");
            var result = await _adminBranchService.GetBranchSummaryAsync();
            return Ok(result);
        }
    }
}
