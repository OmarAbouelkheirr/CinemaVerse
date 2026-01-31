using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminPayment.Responses;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
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
            _logger.LogInformation("Admin: Getting all Payments, Page {Page}, PageSize {PageSize}", filter?.Page ?? 1, filter?.PageSize ?? 10);
            var result = await _adminPaymentService.GetAllPaymentsAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PaymentDetailsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaymentById([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Getting Payment by ID: {PaymentId}", id);
            var result = await _adminPaymentService.GetPaymentByIdAsync(id);
            return Ok(result);
        }
    }
}
