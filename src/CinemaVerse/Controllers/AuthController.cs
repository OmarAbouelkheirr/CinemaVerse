using CinemaVerse.Models;
using CinemaVerse.Services.DTOs.UserFlow.Auth;
using CinemaVerse.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CinemaVerse.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var userId = await _authService.RegisterAsync(request);
            _logger.LogInformation("User registered successfully with Id {UserId}", userId);
            return StatusCode(StatusCodes.Status201Created, new { userId });
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            _logger.LogInformation("User logged in successfully with Id {UserId}", result.UserId);
            return Ok(result);
        }

        [HttpGet("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { error = "Verification token is required." });

            var success = await _authService.VerifyEmailAsync(token);
            if (!success)
                return BadRequest(new { error = "Invalid or expired verification token." });

            return Ok(new { message = "Email verified successfully." });
        }

        [HttpPost("resend-verification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequestDto request)
        {
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);

            var success = await _authService.ResendEmailVerificationAsync(request.Email);
            if (!success)
                return BadRequest(new { error = "Could not resend verification. Check that the email is registered and not already verified." });

            return Ok(new { message = "If the email is registered and not yet verified, a new verification link has been sent." });
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);

            await _authService.RequestPasswordResetAsync(request.Email);
            return Ok(new { message = "If an account exists with this email, a password reset link has been sent." });
        }

        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
                return this.BadRequestFromValidation(ModelState);

            var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);
            if (!success)
                return BadRequest(new { error = "Invalid or expired reset token." });

            return Ok(new { message = "Password has been reset successfully." });
        }
    }
}
