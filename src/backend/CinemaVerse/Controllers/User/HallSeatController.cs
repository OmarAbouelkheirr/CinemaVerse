using CinemaVerse.Services.DTOs.HallSeat.Responses;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.User
{
    [AllowAnonymous]

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
            _logger.LogInformation("User: Getting hall with seats for MovieShowTimeId: {MovieShowTimeId}", movieShowTimeId);
            var result = await _hallSeatService.GetHallWithSeatsAsync(movieShowTimeId);
            _logger.LogInformation("User: Successfully retrieved hall with seats for MovieShowTimeId: {MovieShowTimeId}", movieShowTimeId);
            return Ok(result);
        }
    }
}
