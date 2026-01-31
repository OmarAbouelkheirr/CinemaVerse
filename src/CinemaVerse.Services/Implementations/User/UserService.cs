using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.UserFlow.Profile.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Profile.Responses;
using CinemaVerse.Services.Interfaces.User;
using CinemaVerse.Services.Mappers;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.User
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<UserProfileDto?> GetProfileAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("GetProfileAsync: invalid userId {UserId}", userId);
                return null;
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("GetProfileAsync: user not found for userId {UserId}", userId);
                return null;
            }

            return UserMapper.ToUserProfileDto(user);
        }

        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (userId <= 0)
            {
                _logger.LogWarning("UpdateProfileAsync: invalid userId {UserId}", userId);
                return false;
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("UpdateProfileAsync: user not found for userId {UserId}", userId);
                return false;
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            user.Address = request.Address.Trim();
            user.City = request.City.Trim();
            user.DateOfBirth = request.DateOfBirth;
            user.Gender = request.Gender;
            user.LastUpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Profile updated for userId {UserId}", userId);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("ChangePasswordAsync: invalid userId {UserId}", userId);
                return false;
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("ChangePasswordAsync: user not found for userId {UserId}", userId);
                return false;
            }

            bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash);
            if (!isCurrentPasswordValid)
            {
                _logger.LogWarning("ChangePasswordAsync: invalid current password for userId {UserId}", userId);
                return false;
            }

            if (BCrypt.Net.BCrypt.Verify(newPassword, user.PasswordHash))
            {
                _logger.LogWarning("ChangePasswordAsync: new password same as current for userId {UserId}", userId);
                throw new InvalidOperationException("New password must be different from current password.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.LastUpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Password changed for userId {UserId}", userId);
            return true;
        }
    }
}
