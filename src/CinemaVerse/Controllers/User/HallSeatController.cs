using CinemaVerse.Services.DTOs.HallSeat.Responses;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.User
{
    [Route("api/showtimes")]
    [ApiController]
    public class HallSeatController : ControllerBase
    {
        private readonly ILogger<HallSeatController> _logger;
        private readonly IHallSeatService _hallSeatService;
        public HallSeatController(IHallSeatService hallSeatService, ILogger<HallSeatController> logger)
        {
            _hallSeatService = hallSeatService;
            _logger = logger;

        }
        [HttpGet("{movieShowTimeId}/hall-seats")]
        [ProducesResponseType(typeof(HallWithSeatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHallWithSeats([FromRoute] int movieShowTimeId)
        {
            try
            {
                _logger.LogInformation("User: Getting hall with seats for MovieShowTimeId: {MovieShowTimeId}", movieShowTimeId);
                if (movieShowTimeId <= 0)
                {
                    _logger.LogWarning("Invalid MovieShowTimeId: {MovieShowTimeId}", movieShowTimeId);
                    return BadRequest(new { error = "Invalid MovieShowTimeId" });
                }
                var result = await _hallSeatService.GetHallWithSeatsAsync(movieShowTimeId);
                _logger.LogInformation("User: Successfully retrieved hall with seats for MovieShowTimeId: {MovieShowTimeId}", movieShowTimeId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters");
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "MovieShowTime not found");
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving hall with seats");
                return StatusCode(500, new { error = "An error occurred while retrieving hall with seats" });
            }
        }

    }
}
