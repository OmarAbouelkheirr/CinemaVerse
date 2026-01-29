using CinemaVerse.Extensions;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.UserFlow.Review.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Review.Responses;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.User
{
    [Route("api")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ILogger<ReviewController> _logger;
        private readonly IReviewService _reviewService;

        public ReviewController(ILogger<ReviewController> logger, IReviewService reviewService)
        {
            _logger = logger;
            _reviewService = reviewService;
        }

        [HttpPost("users/{userId:int}/reviews")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateReview([FromRoute] int userId, [FromBody] CreateReviewRequestDto dto)
        {
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);
            _logger.LogInformation("User {UserId}: Creating review for movie {MovieId}", userId, dto.MovieId);
            var result = await _reviewService.CreateReviewAsync(userId, dto);
            _logger.LogInformation("User {UserId}: Successfully created review for movie {MovieId}", userId, dto.MovieId);
            return Ok(result);
        }

        [HttpPut("users/{userId:int}/reviews/{reviewId:int}")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateReview([FromRoute] int userId, [FromRoute] int reviewId, [FromBody] UpdateReviewRequestDto dto)
        {
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);
            _logger.LogInformation("User {UserId}: Updating review {ReviewId}", userId, reviewId);
            var result = await _reviewService.UpdateReviewAsync(userId, reviewId, dto);
            _logger.LogInformation("User {UserId}: Successfully updated review {ReviewId}", userId, reviewId);
            return Ok(result);
        }

        [HttpDelete("users/{userId:int}/reviews/{reviewId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteReview([FromRoute] int userId, [FromRoute] int reviewId)
        {
            _logger.LogInformation("User {UserId}: Deleting review {ReviewId}", userId, reviewId);
            await _reviewService.DeleteReviewAsync(userId, reviewId);
            _logger.LogInformation("User {UserId}: Successfully deleted review {ReviewId}", userId, reviewId);
            return Ok(new { message = "Review deleted successfully" });
        }

        [HttpGet("users/{userId:int}/reviews")]
        [ProducesResponseType(typeof(PagedResultDto<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserReviews(
            [FromRoute] int userId,
            [FromQuery] int? page,
            [FromQuery] int? pageSize)
        {
            var pageValue = page is null or <= 0 ? 1 : page.Value;
            var pageSizeValue = pageSize is null or <= 0 || pageSize > 100 ? 20 : pageSize.Value;
            _logger.LogInformation("Getting reviews for user {UserId} (page {Page}, pageSize {PageSize})", userId, pageValue, pageSizeValue);
            var result = await _reviewService.GetUserReviewsAsync(userId, pageValue, pageSizeValue);
            _logger.LogInformation("Successfully retrieved {Count} reviews for user {UserId}", result.Items.Count, userId);
            return Ok(result);
        }

        [HttpGet("movies/{movieId:int}/reviews")]
        [ProducesResponseType(typeof(PagedResultDto<ReviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetReviewsByMovieId(
            [FromRoute] int movieId,
            [FromQuery] int? page,
            [FromQuery] int? pageSize)
        {
            var pageValue = page is null or <= 0 ? 1 : page.Value;
            var pageSizeValue = pageSize is null or <= 0 || pageSize > 100 ? 20 : pageSize.Value;
            _logger.LogInformation("Getting reviews for movie {MovieId}", movieId);
            var result = await _reviewService.GetReviewsByMovieIdAsync(movieId, pageValue, pageSizeValue);
            _logger.LogInformation("Successfully retrieved reviews for movie {MovieId}", movieId);
            return Ok(result);
        }

        [HttpGet("users/{userId:int}/reviews/movie/{movieId:int}")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserReviewForMovie([FromRoute] int userId, [FromRoute] int movieId)
        {
            _logger.LogInformation("Getting user {UserId} review for movie {MovieId}", userId, movieId);
            var result = await _reviewService.GetUserReviewForMovieAsync(userId, movieId);
            if (result == null)
                return NoContent();
            _logger.LogInformation("Successfully retrieved user review for movie {MovieId}", movieId);
            return Ok(result);
        }
    }
}
