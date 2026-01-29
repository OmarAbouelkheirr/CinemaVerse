using CinemaVerse.Services.DTOs.Payment.NewFolder;
using CinemaVerse.Services.DTOs.Payment.Requests;
using CinemaVerse.Services.DTOs.Payment.Response;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.User
{
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
            try
            {
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid UserId: {UserId}", userId);
                    return BadRequest(new { error = "Invalid UserId" });
                }

                if (createPaymentDto == null)
                {
                    _logger.LogWarning("User: CreatePaymentIntent called with null request");
                    return BadRequest(new { error = "Request body is required" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreatePaymentIntentRequestDto");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("User: Creating payment intent for UserId: {UserId} with details: {@createPaymentDto}", userId, createPaymentDto);
                var result = await _paymentService.CreatePaymentIntent(userId, createPaymentDto);
                _logger.LogInformation("User: Successfully created payment intent for UserId: {UserId}", userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found while creating payment intent");
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized payment attempt");
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while creating payment intent");
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                return StatusCode(500, new
                {
                    error = "An error occurred while creating payment intent",
                    traceId = HttpContext.TraceIdentifier
                });
            }
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
            try
            {
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid UserId: {UserId}", userId);
                    return BadRequest(new { error = "Invalid UserId" });
                }

                if (confirmPaymentDto == null)
                {
                    _logger.LogWarning("User: ConfirmPayment called with null request");
                    return BadRequest(new { error = "Request body is required" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for ConfirmPaymentRequestDto");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("User: Confirming payment for UserId: {UserId} with details: {@confirmPaymentDto}", userId, confirmPaymentDto);
                var result = await _paymentService.ConfirmPaymentAsync(userId, confirmPaymentDto);
                _logger.LogInformation("User: Successfully confirmed payment for UserId: {UserId}", userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found while confirming payment");
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized payment confirmation attempt");
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while confirming payment");
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment");
                return StatusCode(500, new
                {
                    error = "An error occurred while confirming payment",
                    traceId = HttpContext.TraceIdentifier
                });
            }
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
            try
            {
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid UserId: {UserId}", userId);
                    return BadRequest(new { error = "Invalid UserId" });
                }

                if (refundPaymentDto == null)
                {
                    _logger.LogWarning("User: RefundPayment called with null request");
                    return BadRequest(new { error = "Request body is required" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for RefundPaymentRequestDto");
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("User: Processing refund for UserId: {UserId} with details: {@refundPaymentDto}", userId, refundPaymentDto);
                var result = await _paymentService.RefundPaymentForUserAsync(userId, refundPaymentDto);
                _logger.LogInformation("User: Successfully processed refund for UserId: {UserId}", userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found while processing refund");
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized refund attempt");
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while processing refund");
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund");
                return StatusCode(500, new
                {
                    error = "An error occurred while processing refund",
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

    }
}
