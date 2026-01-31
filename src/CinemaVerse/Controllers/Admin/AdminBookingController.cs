using CinemaVerse.Extensions;
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
            _logger.LogInformation("Admin: Getting all Bookings with filter: {@Filter}", filter);
            var result = await _adminBookingService.GetAllBookingsAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BookingDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBookingById([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Getting Booking by ID: {BookingId}", id);
            var result = await _adminBookingService.GetBookingByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BookingDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequestDto createBookingDto)
        {
            _logger.LogInformation("Admin: Creating a new Booking: {@CreateBookingDto}", createBookingDto);
            var bookingId = await _adminBookingService.CreateBookingAsync(createBookingDto);
            _logger.LogInformation("Booking created successfully with ID: {BookingId}", bookingId);
            var result = await _adminBookingService.GetBookingByIdAsync(bookingId);
            return CreatedAtAction(nameof(GetBookingById), new { id = bookingId }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditBooking([FromRoute] int id, [FromBody] UpdateBookingRequestDto updateBookingDto)
        {
            _logger.LogInformation("Admin: Updating Booking with ID: {BookingId}", id);
            await _adminBookingService.UpdateBookingStatusAsync(id, updateBookingDto.NewStatus);
            _logger.LogInformation("Booking with ID {BookingId} updated successfully", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteBooking([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Deleting Booking with ID: {BookingId}", id);
            await _adminBookingService.DeleteBookingAsync(id);
            _logger.LogInformation("Booking with ID {BookingId} deleted successfully", id);
            return NoContent();
        }
    }
}
