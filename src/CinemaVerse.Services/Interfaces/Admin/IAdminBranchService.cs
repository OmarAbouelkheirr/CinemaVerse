using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Response;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminBranchService
    {
        public Task<int> CreateBranchAsync(CreateBranchRequestDto request);
        public Task<int> EditBranchAsync(int branchId, UpdateBranchRequestDto request);
        public Task DeleteBranchAsync(int branchId);
        public Task<PagedResultDto<BranchDetailsResponseDto>> GetAllBranchesAsync(AdminBranchFilterDto filter);
        public Task<BranchDetailsResponseDto?> GetBranchByIdAsync(int branchId);
    }
}
