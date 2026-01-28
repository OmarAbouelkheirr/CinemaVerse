using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Requests
{
    public class UpdateUserRequestDto
    {
        public string? Email { get; set; } = string.Empty;
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; } = string.Empty;
        public string? City { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsEmailConfirmed { get; set; }
        public Genders? Gender { get; set; }

    }
}
