using CinemaVerse.Data.Models.Users;
using CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Responses;
using CinemaVerse.Services.DTOs.Email.Requests;

namespace CinemaVerse.Services.Mappers
{
    public static class UserMapper
    {
        public static UserDetailsDto ToUserDetailsDto(User user)
        {
            return new UserDetailsDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                DateOfBirth = user.DateOfBirth,
                IsActive = user.IsActive,
                IsEmailConfirmed = user.IsEmailConfirmed,
                Gender = user.Gender,
                CreatedAt = user.CreatedAt
            };
        }

        public static WelcomeEmailDto ToWelcomeEmailDto(User user, string subject = "Welcome to CinemaVerse")
        {
            return new WelcomeEmailDto
            {
                To = user.Email,
                FullName = user.FullName,
                Subject = subject
            };
        }
    }
}
