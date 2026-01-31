using CinemaVerse.Services.DTOs.UserFlow.Auth;

namespace CinemaVerse.Services.Interfaces
{
    public interface IAuthService
    {
        Task<int> RegisterAsync(RegisterRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        
        // Email verification
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> ResendEmailVerificationAsync(string email);

        // Password reset
        Task<bool> RequestPasswordResetAsync(string email);   // sends email with reset token/link
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}
