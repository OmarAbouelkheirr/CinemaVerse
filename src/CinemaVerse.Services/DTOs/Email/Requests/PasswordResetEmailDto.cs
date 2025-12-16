using CinemaVerse.Services.DTOs.Email.Flow;

namespace CinemaVerse.Services.DTOs.Email.Requests
{
    public class PasswordResetEmailDto : BaseEmailDto
    {
        public string ResetToken { get; set; } = string.Empty;
        public string ResetLink { get; set; } = string.Empty;
        public DateTime ExpirationTime { get; set; }
    }
}
