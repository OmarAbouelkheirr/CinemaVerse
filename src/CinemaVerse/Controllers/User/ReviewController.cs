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
        public async Task<IActionResult> CreateReview([FromRoute] int userId,[FromBody] CreateReviewRequestDto dto)
        {
            try
            {
                if (dto == null)
                {
                    _logger.LogWarning("CreateReview called with null body");
                    return BadRequest(new { error = "Request body cannot be null" });
                }
                if (userId <= 0)
                    return BadRequest(new { error = "Invalid user ID" });

                _logger.LogInformation("User {UserId}: Creating review for movie {MovieId}", userId, dto.MovieId);
                var result = await _reviewService.CreateReviewAsync(userId, dto);
                _logger.LogInformation("User {UserId}: Successfully created review for movie {MovieId}", userId, dto.MovieId);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Invalid request");
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found");
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User already reviewed this movie");
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, new { error = "An error occurred while saving the review" });
            }
        }

        [HttpPut("users/{userId:int}/reviews/{reviewId:int}")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateReview([FromRoute] int userId,[FromRoute] int reviewId,[FromBody] UpdateReviewRequestDto dto)
        {
            try
            {
                if (dto == null)
                {
                    _logger.LogWarning("UpdateReview called with null body");
                    return BadRequest(new { error = "Request body cannot be null" });
                }
                if (userId <= 0)
                    return BadRequest(new { error = "Invalid user ID" });
                if (reviewId <= 0)
                    return BadRequest(new { error = "Invalid review ID" });

                _logger.LogInformation("User {UserId}: Updating review {ReviewId}", userId, reviewId);
                var result = await _reviewService.UpdateReviewAsync(userId, reviewId, dto);
                _logger.LogInformation("User {UserId}: Successfully updated review {ReviewId}", userId, reviewId);
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "Invalid request");
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Review not found");
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User not allowed to update this review");
                return StatusCode(403, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review");
                return StatusCode(500, new { error = "An error occurred while updating the review" });
            }
        }

        [HttpDelete("users/{userId:int}/reviews/{reviewId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteReview([FromRoute] int userId,[FromRoute] int reviewId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new { error = "Invalid user ID" });
                if (reviewId <= 0)
                    return BadRequest(new { error = "Invalid review ID" });

                _logger.LogInformation("User {UserId}: Deleting review {ReviewId}", userId, reviewId);
                await _reviewService.DeleteReviewAsync(userId, reviewId);
                _logger.LogInformation("User {UserId}: Successfully deleted review {ReviewId}", userId, reviewId);
                return Ok(new { message = "Review deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Review not found");
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User not allowed to delete this review");
                return StatusCode(403, new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review");
                return StatusCode(500, new { error = "An error occurred while deleting the review" });
            }
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
            try
            {
                if (userId <= 0)
                    return BadRequest(new { error = "Invalid user ID" });

                var pageValue = page is null or <= 0 ? 1 : page.Value;
                var pageSizeValue = pageSize is null or <= 0 || pageSize > 100 ? 20 : pageSize.Value;

                _logger.LogInformation("Getting reviews for user {UserId} (page {Page}, pageSize {PageSize})", userId, pageValue, pageSizeValue);
                var result = await _reviewService.GetUserReviewsAsync(userId, pageValue, pageSizeValue);
                _logger.LogInformation("Successfully retrieved {Count} reviews for user {UserId}", result.Items.Count, userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request");
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "User not found");
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user reviews");
                return StatusCode(500, new { error = "An error occurred while retrieving user reviews" });
            }
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
            try
            {
                if (movieId <= 0)
                    return BadRequest(new { error = "Invalid movie ID" });

                // Apply defaults (consistent with rest of project)
                var pageValue = page is null or <= 0 ? 1 : page.Value;
                var pageSizeValue = pageSize is null or <= 0 || pageSize > 100 ? 20 : pageSize.Value;

                _logger.LogInformation("Getting reviews for movie {MovieId}", movieId);
                var result = await _reviewService.GetReviewsByMovieIdAsync(movieId, pageValue, pageSizeValue);
                _logger.LogInformation("Successfully retrieved reviews for movie {MovieId}", movieId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for movie {MovieId}", movieId);
                return StatusCode(500, new { error = "An error occurred while retrieving reviews" });
            }
        }

        [HttpGet("users/{userId:int}/reviews/movie/{movieId:int}")]
        [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserReviewForMovie([FromRoute] int userId,[FromRoute] int movieId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest(new { error = "Invalid user ID" });
                if (movieId <= 0)
                    return BadRequest(new { error = "Invalid movie ID" });

                _logger.LogInformation("Getting user {UserId} review for movie {MovieId}", userId, movieId);
                var result = await _reviewService.GetUserReviewForMovieAsync(userId, movieId);
                if (result == null)
                    return NoContent();
                _logger.LogInformation("Successfully retrieved user review for movie {MovieId}", movieId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user review for movie {MovieId}", movieId);
                return StatusCode(500, new { error = "An error occurred while retrieving the review" });
            }
        }
    }
}
