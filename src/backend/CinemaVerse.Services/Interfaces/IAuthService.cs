using CinemaVerse.Services.DTOs.UserFlow.Auth;

namespace CinemaVerse.Services.Interfaces
{
    public interface IAuthService
    {
        Task<int> RegisterAsync(RegisterRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshRequestDto request);
        Task<bool> LogoutAsync(RefreshRequestDto request);
        
        Task<bool> VerifyEmailAsync(string token);
        Task<bool> ResendEmailVerificationAsync(string email);

        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
    }
}
