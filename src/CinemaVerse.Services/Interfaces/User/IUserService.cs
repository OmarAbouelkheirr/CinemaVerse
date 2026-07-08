using CinemaVerse.Services.DTOs.UserFlow.Profile.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Profile.Responses;

namespace CinemaVerse.Services.Interfaces.User
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequestDto request);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
