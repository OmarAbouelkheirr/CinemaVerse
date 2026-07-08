using CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminUser.Responses;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminUserService
    {
        Task<int> CreateUserAsync(CreateUserRequestDto request);
        Task<int> UpdateUserAsync(int userId, UpdateUserRequestDto request);
        Task DeleteUserAsync(int userId);

        Task<UserDetailsDto?> GetUserByIdAsync(int userId);
        Task<PagedResultDto<UserDetailsDto>> GetAllUsersAsync(AdminUserFilterDto filter);

        Task<bool> ActivateUserAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);

    }
}
