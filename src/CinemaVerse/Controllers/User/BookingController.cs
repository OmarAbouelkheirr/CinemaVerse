using CinemaVerse.Extensions;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.User
{
    [Authorize]

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
            _logger.LogInformation("User: Creating booking: {@CreateBookingDto}", createBookingDto);
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);
            var result = await _bookingService.CreateBookingAsync(createBookingDto);
            _logger.LogInformation("User: Successfully created booking {BookingId} for UserId: {UserId}",
                result.BookingId, createBookingDto.UserId);
            return CreatedAtAction(
                nameof(GetUserBookingById),
                new { bookingId = result.BookingId, userId = createBookingDto.UserId },
                result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<BookingListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserBookings([FromQuery] int userId)
        {
            _logger.LogInformation("User: Getting bookings for UserId: {UserId}", userId);
            var result = await _bookingService.GetUserBookingsAsync(userId);
            _logger.LogInformation("User: Successfully retrieved bookings for UserId: {UserId}", userId);
            return Ok(result);
        }

        [HttpGet("user/{userId:int}")]
        [ProducesResponseType(typeof(List<BookingListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserBookingsByUserId([FromRoute] int userId)
        {
            _logger.LogInformation("User: Getting bookings for UserId (route): {UserId}", userId);
            var result = await _bookingService.GetUserBookingsAsync(userId);
            _logger.LogInformation("User: Successfully retrieved bookings for UserId (route): {UserId}", userId);
            return Ok(result);
        }

        [HttpGet("{bookingId:int}")]
        [ProducesResponseType(typeof(BookingDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserBookingById([FromQuery] int userId, [FromRoute] int bookingId)
        {
            _logger.LogInformation("User: Getting booking details for BookingId: {BookingId} and UserId: {UserId}", bookingId, userId);
            var result = await _bookingService.GetUserBookingByIdAsync(userId, bookingId);
            _logger.LogInformation("User: Successfully retrieved booking details for BookingId: {BookingId} and UserId: {UserId}", bookingId, userId);
            return Ok(result);
        }

        [HttpDelete("{bookingId:int}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelUserBooking([FromQuery] int userId, [FromRoute] int bookingId)
        {
            _logger.LogInformation("User: Cancelling booking for BookingId: {BookingId} and UserId: {UserId}", bookingId, userId);
            var result = await _bookingService.CancelUserBookingAsync(userId, bookingId);
            _logger.LogInformation("User: Successfully cancelled booking for BookingId: {BookingId} and UserId: {UserId}", bookingId, userId);
            return Ok(result);
        }
    }
}
