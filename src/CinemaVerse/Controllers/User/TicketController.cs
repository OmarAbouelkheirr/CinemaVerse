using CinemaVerse.Services.DTOs.Ticket.Response;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.User
{
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
        [ProducesResponseType(typeof(List<TicketListItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserTickets([FromRoute] int userId)
        {
            try
            {
                _logger.LogInformation("User: Getting tickets list for UserId: {UserId}", userId);
                
                if (userId <= 0)
                {
                    _logger.LogWarning("Invalid UserId: {UserId}", userId);
                    return BadRequest(new { error = "Invalid UserId" });
                }

                var result = await _ticketService.GetUserTicketsAsync(userId);
                _logger.LogInformation("User: Successfully retrieved {Count} tickets for UserId: {UserId}", result.Count, userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user tickets");
                return StatusCode(500, new
                {
                    error = "An error occurred while retrieving user tickets",
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet("user/{userId:int}/{ticketId:int}")]
        [ProducesResponseType(typeof(TicketDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserTicketById([FromRoute] int userId, [FromRoute] int ticketId)
        {
            try
            {
                _logger.LogInformation("User: Getting ticket details for TicketId: {TicketId}, UserId: {UserId}", ticketId, userId);
                
                if (userId <= 0 || ticketId <= 0)
                {
                    _logger.LogWarning("Invalid UserId: {UserId} or TicketId: {TicketId}", userId, ticketId);
                    return BadRequest(new { error = "Invalid UserId or TicketId" });
                }

                var result = await _ticketService.GetUserTicketByIdAsync(userId, ticketId);
                
                if (result == null)
                {
                    _logger.LogWarning("Ticket with ID {TicketId} not found for UserId: {UserId}", ticketId, userId);
                    return NotFound(new { error = "Ticket not found." });
                }

                _logger.LogInformation("User: Successfully retrieved ticket details for TicketId: {TicketId}, UserId: {UserId}", ticketId, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized ticket access");
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket details");
                return StatusCode(500, new
                {
                    error = "An error occurred while retrieving ticket details",
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}
