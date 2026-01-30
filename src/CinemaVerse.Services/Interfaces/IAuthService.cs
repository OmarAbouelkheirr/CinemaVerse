using CinemaVerse.Services.DTOs.UserFlow.Auth;

namespace CinemaVerse.Services.Interfaces
{
    public interface IAuthService
    {
        Task<int> RegisterAsync(RegisterRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    }
}
