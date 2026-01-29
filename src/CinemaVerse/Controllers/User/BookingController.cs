using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.User
{
    [Route("api/bookings")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly ILogger<BookingController> _logger;
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto createBookingDto)
        {
            try
            {
                _logger.LogInformation("User: Creating booking: {@CreateBookingDto}", createBookingDto);

                if (createBookingDto == null)
                {
                    _logger.LogWarning("Null request received for CreateBookingRequestDto");
                    return BadRequest(new { error = "Request body is required" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateBookingRequestDto: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                if (createBookingDto.UserId <= 0)
                {
                    _logger.LogWarning("Invalid UserId: {UserId}", createBookingDto.UserId);
                    return BadRequest(new { error = "Invalid UserId" });
                }

                var result = await _bookingService.CreateBookingAsync(createBookingDto);
                _logger.LogInformation("User: Successfully created booking {BookingId} for UserId: {UserId}",
                    result.BookingId, createBookingDto.UserId);

                return CreatedAtAction(
                    nameof(GetUserBookingById),
                    new { bookingId = result.BookingId, userId = createBookingDto.UserId },
                    result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Requested resource not found while creating booking");
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while creating booking");
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, new { error = "An error occurred while creating booking" });
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BookingListDto>), StatusCodes.Status200OK)]    
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserBookings([FromQuery] int userId)
        {
            try
            {
                _logger.LogInformation("User: Getting bookings for UserId: {UserId}", userId);
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid UserId: {UserId}", userId);
                    return BadRequest(new { error = "Invalid UserId" });
                }
                var result = await _bookingService.GetUserBookingsAsync(userId);
                _logger.LogInformation("User: Successfully retrieved bookings for UserId: {UserId}", userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found while retrieving bookings");
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user bookings");
                return StatusCode(500, new
                {
                    error = "An error occurred while retrieving user bookings",
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }


        [HttpGet("user/{userId:int}")]
        [ProducesResponseType(typeof(List<BookingListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserBookingsByUserId([FromRoute] int userId)
        {
            try
            {
                _logger.LogInformation("User: Getting bookings for UserId (route): {UserId}", userId);
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid UserId (route): {UserId}", userId);
                    return BadRequest(new { error = "Invalid UserId" });
                }

                var result = await _bookingService.GetUserBookingsAsync(userId);
                _logger.LogInformation("User: Successfully retrieved bookings for UserId (route): {UserId}", userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found while retrieving bookings (route)");
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user bookings (route)");
                return StatusCode(500, new
                {
                    error = "An error occurred while retrieving user bookings",
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet("{bookingId:int}")]
        [ProducesResponseType(typeof(BookingDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserBookingById([FromQuery] int userId, [FromRoute] int bookingId)
        {
            try
            {
                _logger.LogInformation("User: Getting booking details for BookingId: {BookingId} and UserId: {UserId}", bookingId, userId);
                if (userId <= 0 || bookingId <= 0)
                {
                    _logger.LogWarning("Invalid UserId: {UserId} or BookingId: {BookingId}", userId, bookingId);
                    return BadRequest(new { error = "Invalid UserId or BookingId" });
                }
                var result = await _bookingService.GetUserBookingByIdAsync(userId, bookingId);
                _logger.LogInformation("User: Successfully retrieved booking details for BookingId: {BookingId} and UserId: {UserId}", bookingId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Booking not found");
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized booking access");
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking details");
                return StatusCode(500, new
                {
                    error = "An error occurred while retrieving booking details",
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpDelete("{bookingId:int}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelUserBooking([FromQuery] int userId, [FromRoute] int bookingId)
        {
            try
            {
                _logger.LogInformation("User: Cancelling booking for BookingId: {BookingId} and UserId: {UserId}", bookingId, userId);
                if (userId <= 0 || bookingId <= 0)
                {
                    _logger.LogWarning("Invalid UserId: {UserId} or BookingId: {BookingId}", userId, bookingId);
                    return BadRequest(new { error = "Invalid UserId or BookingId" });
                }
                var result = await _bookingService.CancelUserBookingAsync(userId, bookingId);
                _logger.LogInformation("User: Successfully cancelled booking for BookingId: {BookingId} and UserId: {UserId}", bookingId, userId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Booking not found while cancelling");
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized cancel attempt");
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation while cancelling booking");
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                return StatusCode(500, new
                {
                    error = "An error occurred while cancelling booking",
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }
    
    }

}