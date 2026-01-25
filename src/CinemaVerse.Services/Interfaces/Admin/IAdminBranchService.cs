using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Response;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminBranchService
    {
        public Task<int> CreateBranchAsync(CreateBranchRequestDto Request);
        public Task<int> EditBranchAsync(int branchId, UpdateBranchRequestDto Request);
        public Task DeleteBranchAsync(int branchId);
        public Task<PagedResultDto<BranchDetailsResponseDto>> GetAllBranchesAsync(AdminBranchFilterDto Filter);
        public Task<BranchDetailsResponseDto?> GetBranchByIdAsync(int branchId);
    }
}
