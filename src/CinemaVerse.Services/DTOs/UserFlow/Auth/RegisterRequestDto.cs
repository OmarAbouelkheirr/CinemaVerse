using System.ComponentModel.DataAnnotations;
using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.UserFlow.Auth
{
    public class RegisterRequestDto : IValidatableObject
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [StringLength(254, ErrorMessage = "Email address is too long")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [MaxLength(100, ErrorMessage = "Password is too long")]
        // At least: 1 uppercase, 1 lowercase, 1 digit, 1 special character
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, a number, and a special character")]
        public string Password { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20, ErrorMessage = "Phone number is too long")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 200 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be between 2 and 100 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [EnumDataType(typeof(Genders), ErrorMessage = "Invalid gender value")]
        public Genders Gender { get; set; }


        // Business Validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var today = DateTime.UtcNow.Date;

            // Prevent future dates
            if (DateOfBirth.Date > today)
            {
                yield return new ValidationResult(
                    "Date of birth cannot be in the future",
                    new[] { nameof(DateOfBirth) }
                );
            }

            // Minimum age requirement
            var minimumAge = 13;
            var minAllowedDate = today.AddYears(-minimumAge);

            if (DateOfBirth.Date > minAllowedDate)
            {
                yield return new ValidationResult(
                    $"User must be at least {minimumAge} years old",
                    new[] { nameof(DateOfBirth) }
                );
            }
        }
    }
}
