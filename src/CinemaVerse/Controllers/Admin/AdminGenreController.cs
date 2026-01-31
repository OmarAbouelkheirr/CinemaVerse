using CinemaVerse.Extensions;
using CinemaVerse.Services.DTOs.AdminFlow.AdminGenre.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminGenre.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("api/admin/genres")]
    [ApiController]
    public class AdminGenreController : ControllerBase
    {
        private readonly ILogger<AdminGenreController> _logger;
        private readonly IAdminGenreService _adminGenreService;

        public AdminGenreController(IAdminGenreService adminGenreService, ILogger<AdminGenreController> logger)
        {
            _adminGenreService = adminGenreService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<GenreDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllGenres([FromQuery] AdminGenreFilterDto filter)
        {
            _logger.LogInformation("Admin: Getting all genres, Page {Page}, PageSize {PageSize}", filter?.Page ?? 1, filter?.PageSize ?? 20);
            var result = await _adminGenreService.GetAllGenresAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GenreDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetGenreById([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Getting genre by ID: {GenreId}", id);
            var result = await _adminGenreService.GetGenreAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(GenreDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateGenre([FromBody] CreateGenreRequestDto createGenreDto)
        {
            if (createGenreDto == null)
                return BadRequest(new { error = "Request body is required." });

            _logger.LogInformation("Admin: Creating new genre");
            var genreId = await _adminGenreService.CreateGenreAsync(createGenreDto);
            _logger.LogInformation("Genre created successfully with ID: {GenreId}", genreId);
            var result = await _adminGenreService.GetGenreAsync(genreId);
            return CreatedAtAction(nameof(GetGenreById), new { id = genreId }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EditGenre([FromRoute] int id, [FromBody] UpdateGenreRequestDto updateGenreDto)
        {
            if (updateGenreDto == null)
                return BadRequest(new { error = "Request body is required." });

            _logger.LogInformation("Admin: Updating genre with ID: {GenreId}", id);
            await _adminGenreService.UpdateGenreAsync(id, updateGenreDto);
            _logger.LogInformation("Genre with ID {GenreId} updated successfully", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteGenre([FromRoute] int id)
        {
            _logger.LogInformation("Admin: Deleting genre with ID: {GenreId}", id);
            await _adminGenreService.DeleteGenreAsync(id);
            _logger.LogInformation("Genre with ID {GenreId} deleted successfully", id);
            return NoContent();
        }
    }
}
