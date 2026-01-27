using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Route("api/admin/branches")]
    [ApiController]
    public class AdminBranchController : ControllerBase
    {
        private readonly ILogger<AdminBranchController> _logger;
        private readonly IAdminBranchService _adminBranchService;

        public AdminBranchController(IAdminBranchService adminBranchService, ILogger<AdminBranchController> logger)
        {
            _adminBranchService = adminBranchService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<BranchDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBranches([FromQuery] AdminBranchFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Admin: Getting all branches with filter: {@Filter}", filter);
                var result = await _adminBranchService.GetAllBranchesAsync(filter);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving branches");
                return StatusCode(500, new { error = "An error occurred while retrieving branches" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BranchDetailsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBranchById([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin: Getting branch by ID: {BranchId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid branch ID: {BranchId}", id);
                    return BadRequest(new { error = "Invalid branch ID" });
                }
                var result = await _adminBranchService.GetBranchByIdAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Branch with ID {BranchId} not found", id);
                    return NotFound(new { error = "Branch not found" });
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
                _logger.LogError(ex, "Error retrieving branch by ID");
                return StatusCode(500, new { error = "An error occurred while retrieving the branch" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequestDto createBranchDto)
        {
            try
            {
                _logger.LogInformation("Admin: Creating a new branch: {@CreateBranchDto}", createBranchDto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateBranchRequestDto: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                var branchId = await _adminBranchService.CreateBranchAsync(createBranchDto);
                _logger.LogInformation("Branch created successfully with ID: {BranchId}", branchId);

                return CreatedAtAction(
                    nameof(GetBranchById),
                    new { id = branchId },
                    new { id = branchId, message = "Branch created successfully" });
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
                _logger.LogError(ex, "Error creating branch");
                return StatusCode(500, new { error = "An error occurred while creating the branch" });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditBranch([FromRoute] int id, [FromBody] UpdateBranchRequestDto updateBranchDto)
        {
            try
            {
                _logger.LogInformation("Admin: Updating branch with ID: {BranchId}", id);
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateBranchRequestDto: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid branch ID: {BranchId}", id);
                    return BadRequest(new { error = "Invalid branch ID" });
                }

                await _adminBranchService.EditBranchAsync(id, updateBranchDto);
                _logger.LogInformation("Branch with ID {BranchId} updated successfully", id);
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null request received for branch Id {BranchId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request data for branch Id {BranchId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Branch with Id {BranchId} not found", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating branch with branch Id {BranchId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the branch" });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBranch([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin: Deleting branch with ID: {BranchId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid branch ID: {BranchId}", id);
                    return BadRequest(new { error = "Invalid branch ID" });
                }
                await _adminBranchService.DeleteBranchAsync(id);
                _logger.LogInformation("Branch with ID {BranchId} deleted successfully", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Branch with ID {BranchId} not found", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting branch with ID {BranchId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the branch" });
            }
        }
    }
}
