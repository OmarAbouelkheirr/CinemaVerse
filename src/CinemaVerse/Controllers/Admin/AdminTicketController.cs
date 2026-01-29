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
            _logger.LogInformation("Admin: Getting all tickets");
            var result = await _adminTicketService.GetAllTicketsAsync(adminTicketFilterDto);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AdminTicketDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTicketById([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Getting ticket by ID: {TicketId}", id);
            var ticket = await _adminTicketService.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound(new { error = "Ticket not found" });
            return Ok(ticket);
        }

        [HttpGet("check-qr")]
        [ProducesResponseType(typeof(TicketCheckResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckTicketByQrToken([FromQuery] string qrToken)
        {
            _logger.LogInformation("Admin: Checking ticket by QR token");
            var result = await _adminTicketService.CheckTicketByQrTokenAsync(qrToken);
            return Ok(result);
        }

        [HttpGet("ByBooking/{bookingId}")]
        [ProducesResponseType(typeof(List<AdminTicketDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketsByBookingIdAsync([FromRoute] int bookingId)
        {
            _logger.LogInformation("Admin: Getting tickets by Booking ID: {BookingId}", bookingId);
            var tickets = await _adminTicketService.GetTicketsByBookingIdAsync(bookingId);
            return Ok(tickets);
        }

        [HttpGet("ByShowtime/{showtimeId}")]
        [ProducesResponseType(typeof(List<AdminTicketDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTicketsByShowtimeIdAsync([FromRoute] int showtimeId)
        {
            _logger.LogInformation("Admin: Getting tickets by Showtime ID: {ShowtimeId}", showtimeId);
            var tickets = await _adminTicketService.GetTicketsByShowtimeIdAsync(showtimeId);
            return Ok(tickets);
        }

        [HttpPost("check-in")]
        [ProducesResponseType(typeof(TicketCheckInResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkTicketAsUsed([FromQuery] string qrToken)
        {
            _logger.LogInformation("Admin: Marking ticket as used by QR token");
            var result = await _adminTicketService.MarkTicketAsUsedAsync(qrToken);
            return Ok(result);
        }
    }
}
