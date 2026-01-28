using CinemaVerse.Services.DTOs.AdminFlow.AdminBooking.Requests;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Route("api/admin/bookings")]
    [ApiController]
    public class AdminBookingController : ControllerBase
    {
        private readonly ILogger<AdminBookingController> _logger;
        private readonly IAdminBookingService _adminBookingService;

        public AdminBookingController(IAdminBookingService adminBookingService, ILogger<AdminBookingController> logger)
        {
            _adminBookingService = adminBookingService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<BookingDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBookings([FromQuery] AdminBookingFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Admin: Getting all Bookings with filter: {@Filter}", filter);
                var result = await _adminBookingService.GetAllBookingsAsync(filter);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Bookinges");
                return StatusCode(500, new { error = "An error occurred while retrieving Bookings" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BookingDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBookingById([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin: Getting Booking by ID: {BookingId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid Booking ID: {BookingId}", id);
                    return BadRequest(new { error = "Invalid Booking ID" });
                }
                var result = await _adminBookingService.GetBookingByIdAsync(id);
                if (result == null)
                {
                    _logger.LogWarning("Booking with ID {BookingId} not found", id);
                    return NotFound(new { error = "Booking not found" });
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
                _logger.LogError(ex, "Error retrieving Booking by ID");
                return StatusCode(500, new { error = "An error occurred while retrieving the Booking" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto createBookingDto)
        {
            try
            {
                _logger.LogInformation("Admin: Creating a new Booking: {@CreateBookingDto}", createBookingDto);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for CreateBookingRequestDto: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }

                var BookingId = await _adminBookingService.CreateBookingAsync(createBookingDto);
                _logger.LogInformation("Booking created successfully with ID: {BookingId}", BookingId);

                return CreatedAtAction(
                    nameof(GetBookingById),
                    new { id = BookingId },
                    new { id = BookingId, message = "Booking created successfully" });
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
                _logger.LogError(ex, "Error creating Booking");
                return StatusCode(500, new { error = "An error occurred while creating the Booking" });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditBooking([FromRoute] int id,[FromBody] UpdateBookingRequestDto updateBookingDto)
        {
            try
            {
                _logger.LogInformation("Admin: Updating Booking with ID: {BookingId}", id);
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state for UpdateBookingRequestDto: {@ModelState}", ModelState);
                    return BadRequest(ModelState);
                }
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid Booking ID: {BookingId}", id);
                    return BadRequest(new { error = "Invalid Booking ID" });
                }

                await _adminBookingService.UpdateBookingStatusAsync(id, updateBookingDto.NewStatus);
                _logger.LogInformation("Booking with ID {BookingId} updated successfully", id);
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Null request received for Booking Id {BookingId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request data for Booking Id {BookingId}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Booking with Id {BookingId} not found", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Booking with Booking Id {BookingId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the Booking" });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBooking([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin: Deleting Booking with ID: {BookingId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid Booking ID: {BookingId}", id);
                    return BadRequest(new { error = "Invalid Booking ID" });
                }
                await _adminBookingService.DeleteBookingAsync(id);
                _logger.LogInformation("Booking with ID {BookingId} deleted successfully", id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Booking with ID {BookingId} not found", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Booking with ID {BookingId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the Booking" });
            }
        }
    }
}
