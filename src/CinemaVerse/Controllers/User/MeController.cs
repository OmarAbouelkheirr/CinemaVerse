using CinemaVerse.Extensions;
using CinemaVerse.Models;
using CinemaVerse.Services.DTOs.UserFlow.Profile.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Profile.Responses;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers.User
{
    [Authorize]
    [Route("api/me")]
    [ApiController]
    public class MeController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<MeController> _logger;

        public MeController(IUserService userService, ILogger<MeController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = this.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { error = new ErrorResponse { Message = "Invalid or missing user identity.", Code = "UNAUTHORIZED" } });

            var profile = await _userService.GetProfileAsync(userId.Value);
            if (profile == null)
                return NotFound(new { error = new ErrorResponse { Message = "User not found.", Code = "NOT_FOUND" } });

            return Ok(profile);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            if (request == null)
                return BadRequest(new { error = new ErrorResponse { Message = "Request body is required.", Code = "VALIDATION_ERROR" } });

            var userId = this.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { error = new ErrorResponse { Message = "Invalid or missing user identity.", Code = "UNAUTHORIZED" } });

            var success = await _userService.UpdateProfileAsync(userId.Value, request);
            if (!success)
                return NotFound(new { error = new ErrorResponse { Message = "User not found.", Code = "NOT_FOUND" } });

            return Ok(new { message = "Profile updated successfully." });
        }

        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            if (request == null)
                return BadRequest(new { error = new ErrorResponse { Message = "Request body is required.", Code = "VALIDATION_ERROR" } });
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);

            var userId = this.GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { error = new ErrorResponse { Message = "Invalid or missing user identity.", Code = "UNAUTHORIZED" } });

            var success = await _userService.ChangePasswordAsync(userId.Value, request.CurrentPassword, request.NewPassword);
            if (!success)
                return BadRequest(new { error = new ErrorResponse { Message = "Current password is incorrect or user not found.", Code = "VALIDATION_ERROR" } });

            return Ok(new { message = "Password changed successfully." });
        }
    }
}
