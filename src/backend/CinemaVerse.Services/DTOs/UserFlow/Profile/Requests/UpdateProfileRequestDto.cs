using System.ComponentModel.DataAnnotations;
using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.UserFlow.Profile.Requests
{
    public class UpdateProfileRequestDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 100 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 100 characters")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 500 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be between 2 and 100 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [EnumDataType(typeof(Genders), ErrorMessage = "Invalid gender")]
        public Genders Gender { get; set; }
    }
}
