using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Route("api/admin/tickets")]
    [ApiController]
    public class AdminTicketController : ControllerBase
    {
        private readonly ILogger<AdminTicketController> _logger;
        private readonly IAdminTicketService _adminTicketService;

        public AdminTicketController(IAdminTicketService adminTicketService, ILogger<AdminTicketController> logger)
        {
            _adminTicketService = adminTicketService;
            _logger = logger;
        }


        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<AdminTicketListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTickets([FromQuery] AdminTicketFilterDto adminTicketFilterDto)
        {
            try
            {
                _logger.LogInformation("Admin: Getting all tickets");
                var result = await _adminTicketService.GetAllTicketsAsync(adminTicketFilterDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets");
                return StatusCode(500, new { error = "An error occurred while retrieving tickets" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AdminTicketDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTicketById([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Admin: Getting ticket by ID: {TicketId}", id);
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid ticket ID: {TicketId}", id);
                    return BadRequest(new { error = "Invalid ticket ID" });
                }
                var ticket = await _adminTicketService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    _logger.LogWarning("Ticket not found: {TicketId}", id);
                    return NotFound(new { error = "Ticket not found" });
                }
                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket by ID: {TicketId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the ticket" });
            }
        }

        [HttpGet("check-qr")]
        [ProducesResponseType(typeof(TicketCheckResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckTicketByQrToken([FromQuery] string qrToken)
        {
            try
            {
                _logger.LogInformation("Admin: Checking ticket by QR token");
                if (string.IsNullOrWhiteSpace(qrToken))
                {
                    _logger.LogWarning("QR token is null or empty");
                    return BadRequest(new { error = "QR token is required" });
                }
                var result = await _adminTicketService.CheckTicketByQrTokenAsync(qrToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid QR token");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking ticket by QR token");
                return StatusCode(500, new { error = "An error occurred while checking the ticket" });
            }
        }


        [HttpGet("ByBooking/{bookingId}")]
        [ProducesResponseType(typeof(List<AdminTicketDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketsByBookingIdAsync([FromRoute] int bookingId)
        {
            try
            {
                _logger.LogInformation("Admin: Getting tickets by Booking ID: {BookingId}", bookingId);
                if (bookingId <= 0)
                {
                    _logger.LogWarning("Invalid booking ID: {BookingId}", bookingId);
                    return BadRequest(new { error = "Invalid booking ID" });
                }
                var tickets = await _adminTicketService.GetTicketsByBookingIdAsync(bookingId);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets by Booking ID: {BookingId}", bookingId);
                return StatusCode(500, new { error = "An error occurred while retrieving the tickets" });
            }
        }


        [HttpGet("ByShowtime/{showtimeId}")]
        [ProducesResponseType(typeof(List<AdminTicketDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketsByShowtimeIdAsync([FromRoute] int showtimeId)
        {
            try
            {
                _logger.LogInformation("Admin: Getting tickets by Showtime ID: {ShowtimeId}", showtimeId);
                if (showtimeId <= 0)
                {
                    _logger.LogWarning("Invalid showtime ID: {ShowtimeId}", showtimeId);
                    return BadRequest(new { error = "Invalid showtime ID" });
                }
                var tickets = await _adminTicketService.GetTicketsByShowtimeIdAsync(showtimeId);
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets by Showtime ID: {ShowtimeId}", showtimeId);
                return StatusCode(500, new { error = "An error occurred while retrieving the tickets" });
            }
        }


        [HttpPost("check-in")]
        [ProducesResponseType(typeof(TicketCheckInResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkTicketAsUsed([FromQuery] string qrToken)
        {
            try
            {
                _logger.LogInformation("Admin: Marking ticket as used by QR token");
                if (string.IsNullOrWhiteSpace(qrToken))
                {
                    _logger.LogWarning("QR token is null or empty");
                    return BadRequest(new { error = "QR token is required" });
                }
                var result = await _adminTicketService.MarkTicketAsUsedAsync(qrToken);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid QR token");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking ticket as used by QR token");
                return StatusCode(500, new { error = "An error occurred while marking the ticket as used" });
            }

        }

    }
}
