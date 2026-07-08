using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.UserFlow.Auth
{
    public class ForgotPasswordRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = string.Empty;
    }
}
