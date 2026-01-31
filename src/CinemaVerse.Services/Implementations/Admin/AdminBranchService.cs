using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using CinemaVerse.Services.Mappers;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminBranchService : IAdminBranchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminBranchService> _logger;
        public AdminBranchService(IUnitOfWork unitOfWork, ILogger<AdminBranchService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<int> CreateBranchAsync(CreateBranchRequestDto request)
        {
            try
            {
                _logger.LogInformation("Creating Branch {@request}", request);
                var existingBranch = await _unitOfWork.Branches
                    .FirstOrDefaultAsync(b => b.BranchName.ToLower() == request.BranchName.ToLower());
                if (existingBranch != null)
                    throw new InvalidOperationException($"Branch with name {request.BranchName} already exists.");
                var branch = new Branch
                {
                    BranchName = request.BranchName,
                    BranchLocation = request.BranchLocation
                };
                await _unitOfWork.Branches.AddAsync(branch);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Branch created successfully with Id {BranchId}", branch.Id);
                return branch.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating branch {@request}", request);
                throw;
            }
        }

        public async Task DeleteBranchAsync(int branchId)
        {
            try
            {
                _logger.LogInformation("Deleting Branch with branch Id {branchId}", branchId);
                var branch = await _unitOfWork.Branches.GetByIdAsync(branchId);
                if (branch == null)
                    throw new KeyNotFoundException($"Branch with Id {branchId} not found.");
                await _unitOfWork.Branches.DeleteAsync(branch);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Branch with Id {branchId} deleted successfully", branchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting branch with branch Id {branchId}", branchId);
                throw;
            }
        }

        public async Task<int> EditBranchAsync(int branchId, UpdateBranchRequestDto request)
        {
            try
            {
                _logger.LogInformation("Updating Branch with branch Id {branchId}", branchId);
                if (branchId <= 0)
                    throw new ArgumentException("Branch ID must be a positive integer.", nameof(branchId));
                var branch = await _unitOfWork.Branches.GetByIdAsync(branchId);
                if (branch == null)
                    throw new KeyNotFoundException($"Branch with Id {branchId} not found.");
                if (!string.IsNullOrWhiteSpace(request.BranchName))
                {
                    branch.BranchName = request.BranchName;
                }
                if (!string.IsNullOrWhiteSpace(request.BranchLocation))
                {
                    branch.BranchLocation = request.BranchLocation;
                }
                await _unitOfWork.Branches.UpdateAsync(branch);

                _logger.LogInformation("Branch with Id {branchId} updated successfully", branchId);
                return await _unitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating branch with branch Id {branchId}", branchId);
                throw;
            }
        }

        public async Task<PagedResultDto<BranchDetailsResponseDto>> GetAllBranchesAsync(AdminBranchFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting all Branches");
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > PaginationConstants.MaxPageSize)
                    filter.PageSize = PaginationConstants.DefaultPageSize;

                var query = _unitOfWork.Branches.GetQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(b =>
                        b.BranchName.ToLower().Contains(searchLower) ||
                        b.BranchLocation.ToLower().Contains(searchLower));
                }
                var totalCount = await _unitOfWork.Branches.CountAsync(query);

                string sortBy = filter.SortBy?.ToLower() ?? "branchname";
                string sortOrder = filter.SortOrder?.ToLower() ?? "desc";

                if (sortBy == "branchname")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.BranchName)
                        : query.OrderByDescending(b => b.BranchName);
                }
                else if (sortBy == "branchlocation")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.BranchLocation)
                        : query.OrderByDescending(b => b.BranchLocation);
                }
                else // Default
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.BranchName)
                        : query.OrderByDescending(b => b.BranchName);
                }

                var branches = await _unitOfWork.Branches.GetPagedAsync(
                    query: query,
                    orderBy: null,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize
                    );
                var branchDtos = branches.Select(BranchMapper.ToDetailsResponseDto).ToList();
                var pagedResult = new PagedResultDto<BranchDetailsResponseDto>
                {
                    Items = branchDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };
                _logger.LogInformation("Retrieved {Count} branchs", branchDtos.Count);
                return pagedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all branchs");
                throw;
            }
        }

        public async Task<BranchDetailsResponseDto> GetBranchByIdAsync(int branchId)
        {
            try
            {
                _logger.LogInformation("Getting Branch with branch Id {branchId}", branchId);
                if (branchId <= 0)
                    throw new ArgumentException("Branch ID must be a positive integer.", nameof(branchId));
                var branch = await _unitOfWork.Branches.GetByIdAsync(branchId);
                if (branch == null)
                    throw new KeyNotFoundException($"Branch with Id {branchId} not found.");
                var response = BranchMapper.ToDetailsResponseDto(branch);
                _logger.LogInformation("Branch with Id {branchId} retrieved successfully", branchId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branch with branch Id {branchId}", branchId);
                throw;
            }
        }
    }
}
