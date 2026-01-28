using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Route("api/admin/payments")]
    [ApiController]
    public class AdminPaymentController : ControllerBase
    {
        private readonly ILogger<AdminPaymentController> _logger;
        private readonly IAdminPaymentService _adminPaymentService;
        public AdminPaymentController(ILogger<AdminPaymentController> logger, IAdminPaymentService adminPaymentService)
        {
            _logger = logger;
            _adminPaymentService = adminPaymentService;
        }
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<PaymentDetailsResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPayments([FromQuery] AdminPaymentFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Admin: Getting all Payments with filter: {@Filter}", filter);
                var result = await _adminPaymentService.GetAllPaymentsAsync(filter);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Paymentes");
                return StatusCode(500, new { error = "An error occurred while retrieving Payments" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PaymentDetailsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaymentById([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin: Getting Payment by ID: {PaymentId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid Payment ID: {PaymentId}", id);
                    return BadRequest(new { error = "Invalid Payment ID" });
                }
                var result = await _adminPaymentService.GetPaymentByIdAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Payment with ID {PaymentId} not found", id);
                    return NotFound(new { error = "Payment not found" });
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
                _logger.LogError(ex, "Error retrieving Payment by ID");
                return StatusCode(500, new { error = "An error occurred while retrieving the Payment" });
            }
        }

    }
}
