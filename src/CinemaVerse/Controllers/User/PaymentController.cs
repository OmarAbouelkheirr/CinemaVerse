using CinemaVerse.Extensions;
using CinemaVerse.Services.DTOs.UserFlow.Payment.Requests;
using CinemaVerse.Services.DTOs.Payment.Requests;
using CinemaVerse.Services.DTOs.Payment.Response;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.User
{
    [Authorize]
    [Route("api/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IPaymentService _paymentService;

        public PaymentController(ILogger<PaymentController> logger, IPaymentService paymentService)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        [HttpPost("user/{userId:int}/intent")]
        [ProducesResponseType(typeof(CreatePaymentIntentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreatePaymentIntent([FromRoute] int userId, [FromBody] CreatePaymentIntentRequestDto createPaymentDto)
        {
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);
            _logger.LogInformation("User: Creating payment intent for UserId: {UserId} with details: {@createPaymentDto}", userId, createPaymentDto);
            var result = await _paymentService.CreatePaymentIntent(userId, createPaymentDto);
            _logger.LogInformation("User: Successfully created payment intent for UserId: {UserId}", userId);
            return Ok(result);
        }

        [HttpPost("user/{userId:int}/confirm")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmPayment([FromRoute] int userId, [FromBody] ConfirmPaymentRequestDto confirmPaymentDto)
        {
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);
            _logger.LogInformation("User: Confirming payment for UserId: {UserId} with details: {@confirmPaymentDto}", userId, confirmPaymentDto);
            var result = await _paymentService.ConfirmPaymentAsync(userId, confirmPaymentDto);
            _logger.LogInformation("User: Successfully confirmed payment for UserId: {UserId}", userId);
            return Ok(result);
        }

        [HttpPost("user/{userId:int}/refund")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefundPayment([FromRoute] int userId, [FromBody] RefundPaymentRequestDto refundPaymentDto)
        {
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);
            _logger.LogInformation("User: Processing refund for UserId: {UserId} with details: {@refundPaymentDto}", userId, refundPaymentDto);
            var result = await _paymentService.RefundPaymentForUserAsync(userId, refundPaymentDto);
            _logger.LogInformation("User: Successfully processed refund for UserId: {UserId}", userId);
            return Ok(result);
        }
    }
}
