using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.UserFlow.Profile.Responses
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public Genders Gender { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
