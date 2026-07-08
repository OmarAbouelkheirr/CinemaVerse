using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.UserFlow.Auth
{
    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [MaxLength(100, ErrorMessage = "Password is too long")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, a number, and a special character")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
