using CinemaVerse.Extensions;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.Ticket.Response;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.User
{
    [Authorize]
    [Route("api/tickets")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ILogger<TicketController> _logger;
        private readonly ITicketService _ticketService;

        public TicketController(ILogger<TicketController> logger, ITicketService ticketService)
        {
            _logger = logger;
            _ticketService = ticketService;
        }

        [HttpGet("user/{userId:int}")]
        [ProducesResponseType(typeof(PagedResultDto<TicketDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserTickets([FromRoute] int userId, [FromQuery] AdminTicketFilterDto? filter = null)
        {
            var currentUserId = this.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { error = "Invalid or missing user identity." });
            if (userId != currentUserId)
                return Forbid();

            _logger.LogInformation("User: Getting tickets list for UserId: {UserId}", userId);
            var result = await _ticketService.GetUserTicketsAsync(userId, filter ?? new AdminTicketFilterDto());
            _logger.LogInformation("User: Successfully retrieved {Count} tickets for UserId: {UserId}", result.Items.Count, userId);
            return Ok(result);
        }

        [HttpGet("user/{userId:int}/{ticketId:int}")]
        [ProducesResponseType(typeof(TicketDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserTicketById([FromRoute] int userId, [FromRoute] int ticketId)
        {
            var currentUserId = this.GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(new { error = "Invalid or missing user identity." });
            if (userId != currentUserId)
                return Forbid();

            _logger.LogInformation("User: Getting ticket details for TicketId: {TicketId}, UserId: {UserId}", ticketId, userId);
            var result = await _ticketService.GetUserTicketByIdAsync(userId, ticketId);
            _logger.LogInformation("User: Successfully retrieved ticket details for TicketId: {TicketId}, UserId: {UserId}", ticketId, userId);
            return Ok(result);
        }
    }
}
