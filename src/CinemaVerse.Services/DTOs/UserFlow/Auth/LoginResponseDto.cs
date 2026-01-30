using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.UserFlow.Auth
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }  
    }
}