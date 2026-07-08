using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.UserFlow.Movie.Flow;
using CinemaVerse.Services.DTOs.Movie.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Movie.Response;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CinemaVerse.API.Controllers.User
{
    [AllowAnonymous]

    [Route("api/movies")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly ILogger<MovieController> _logger;
        private readonly IMovieService _movieService;

        public MovieController(ILogger<MovieController> logger, IMovieService movieService)
        {
            _logger = logger;
            _movieService = movieService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(BrowseMoviesResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BrowseMovie([FromQuery] BrowseMoviesFilterDto filter)
        {
            if (filter == null)
                filter = new BrowseMoviesFilterDto();
            _logger.LogInformation("User: Browsing movies, Page {Page}, PageSize {PageSize}", filter.Page, filter.PageSize);
            var result = await _movieService.BrowseMoviesAsync(filter);
            _logger.LogInformation("User: Successfully retrieved movies, Page {Page}, PageSize {PageSize}", filter.Page, filter.PageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MovieDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMovieDetails([FromRoute] int id)
        {
            _logger.LogInformation("User: Getting movie details for ID: {MovieId}", id);
            var result = await _movieService.GetMovieDetailsAsync(id);
            _logger.LogInformation("User: Successfully retrieved movie details for ID: {MovieId}", id);
            return Ok(result);
        }
    }
}
