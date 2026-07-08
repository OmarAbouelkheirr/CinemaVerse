using CinemaVerse.Services.DTOs.Email.Flow;

namespace CinemaVerse.Services.DTOs.Email.Requests
{
    public class WelcomeEmailDto : BaseEmailDto
    {
        public string FullName { get; set; } = string.Empty;
    }
}
