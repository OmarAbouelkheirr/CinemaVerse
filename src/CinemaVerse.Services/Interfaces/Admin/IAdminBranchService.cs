using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Response;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminBranchService
    {
        Task<int> CreateBranchAsync(CreateBranchRequestDto request);
        Task<int> EditBranchAsync(int branchId, UpdateBranchRequestDto request);
        Task DeleteBranchAsync(int branchId);
        Task<PagedResultDto<BranchDetailsResponseDto>> GetAllBranchesAsync(AdminBranchFilterDto filter);
        Task<BranchDetailsResponseDto> GetBranchByIdAsync(int branchId);
        Task<BranchSummaryDto> GetBranchSummaryAsync();
    }
}
