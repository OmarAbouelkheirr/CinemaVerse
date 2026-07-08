using CinemaVerse.Services.DTOs.Email.Flow;

namespace CinemaVerse.Services.DTOs.Email.Requests
{
    public class EmailVerificationEmailDto : BaseEmailDto
    {
        public string FullName { get; set; } = string.Empty;
        public string VerificationLink { get; set; } = string.Empty;
        public DateTime ExpirationTime { get; set; }
    }
}
