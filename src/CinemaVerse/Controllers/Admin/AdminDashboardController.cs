using CinemaVerse.Services.DTOs.AdminFlow.AdminDashboard.Response;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/dashboard")]
    [ApiController]
    public class AdminDashboardController : ControllerBase
    {
        private readonly ILogger<AdminDashboardController> _logger;
        private readonly IAdminDashboard _adminDashboard;

        public AdminDashboardController(IAdminDashboard adminDashboard, ILogger<AdminDashboardController> logger)
        {
            _adminDashboard = adminDashboard;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSummary()
        {
            _logger.LogInformation("Admin: Getting dashboard summary.");
            var summary = await _adminDashboard.GetDashboardSummaryAsync();
            return Ok(DashboardSummaryResponseDto.From(summary));
        }

        [HttpGet("total-revenue")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTotalRevenue()
        {
            _logger.LogInformation("Admin: Getting total revenue.");
            var result = await _adminDashboard.GetTotalRevenue();
            return Ok(result);
        }

        [HttpGet("total-bookings")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTotalBookings()
        {
            _logger.LogInformation("Admin: Getting total bookings.");
            var result = await _adminDashboard.GetTotalBookings();
            return Ok(result);
        }

        [HttpGet("active-users")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveUsers()
        {
            _logger.LogInformation("Admin: Getting active users count.");
            var result = await _adminDashboard.GetActiveUsers();
            return Ok(result);
        }

        [HttpGet("occupancy-rate")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOccupancyRate()
        {
            _logger.LogInformation("Admin: Getting occupancy rate.");
            var result = await _adminDashboard.CalculateOccupancyRate();
            return Ok(result);
        }

        [HttpGet("monthly-revenue")]
        [ProducesResponseType(typeof(Dictionary<string, decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMonthlyRevenue()
        {
            _logger.LogInformation("Admin: Getting monthly revenue.");
            var result = await _adminDashboard.GetMonthlyRevenue();
            return Ok(result);
        }

        [HttpGet("weekly-bookings")]
        [ProducesResponseType(typeof(Dictionary<string, decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWeeklyBookings()
        {
            _logger.LogInformation("Admin: Getting weekly bookings.");
            var result = await _adminDashboard.GetWeeklyBookings();
            return Ok(result);
        }
    }
}
