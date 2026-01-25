using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminBranch.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
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
        public async Task<int> CreateBranchAsync(CreateBranchRequestDto Request)
        {
            try
            {
                _logger.LogInformation("Creating Branch {@Request}", Request);
                if (Request == null)
                {
                    _logger.LogWarning("Request cannot be null");
                    throw new ArgumentNullException(nameof(Request), "Request cannot be null.");
                }
                if (string.IsNullOrWhiteSpace(Request.BranchName) || string.IsNullOrWhiteSpace(Request.BranchLocation))
                {
                    _logger.LogWarning("Invalid branch data {@Request}", Request);
                    throw new ArgumentException("Branch name and location cannot be empty.");
                }
                var existingBranch = await _unitOfWork.Branchs
                    .FirstOrDefaultAsync(b => b.BranchName.ToLower() == Request.BranchName.ToLower());
                if (existingBranch != null)
                {
                    _logger.LogWarning("Branch with name {BranchName} already exists", Request.BranchName);
                    throw new InvalidOperationException($"Branch with name {Request.BranchName} already exists.");
                }
                var branch = new Branch
                {
                    BranchName = Request.BranchName,
                    BranchLocation = Request.BranchLocation
                };
                await _unitOfWork.Branchs.AddAsync(branch);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Branch created successfully with Id {BranchId}", branch.Id);
                return branch.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creaing branch {@Request}", Request);
                throw;
            }
        }

        public async Task DeleteBranchAsync(int branchId)
        {
            try
            {
                _logger.LogInformation("Deleting Branch with branch Id {branchId}", branchId);
                var branch = await _unitOfWork.Branchs.GetByIdAsync(branchId);
                if (branch == null)
                {
                    _logger.LogWarning("Branch with Id {branchId} not found", branchId);
                    throw new KeyNotFoundException($"Branch with Id {branchId} not found.");
                }
                await _unitOfWork.Branchs.DeleteAsync(branch);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Branch with Id {branchId} deleted successfully", branchId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting branch with branch Id {branchId}", branchId);
                throw;
            }
        }

        public async Task<int> EditBranchAsync(int branchId, UpdateBranchRequestDto Request)
        {
            try
            {
                _logger.LogInformation("Updating Branch with branch Id {branchId}", branchId);
                if (branchId <= 0)
                {
                    _logger.LogWarning("Invalid branch Id {branchId}", branchId);
                    throw new ArgumentException("Invalid branch Id.");
                }
                var branch = await _unitOfWork.Branchs.GetByIdAsync(branchId);
                if (branch == null)
                {
                    _logger.LogWarning("Branch with Id {branchId} not found", branchId);
                    throw new KeyNotFoundException($"Branch with Id {branchId} not found.");
                }
                if (!string.IsNullOrWhiteSpace(Request.BranchName))
                {
                    branch.BranchName = Request.BranchName;
                }
                if (!string.IsNullOrWhiteSpace(Request.BranchLocation))
                {
                    branch.BranchLocation = Request.BranchLocation;
                }
                await _unitOfWork.Branchs.UpdateAsync(branch);

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
                _logger.LogInformation("Getting all Branchs");
                if (filter == null)
                {
                    _logger.LogWarning("Filter cannot be null");
                    throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
                }
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > 100)
                    filter.PageSize = 20;

                var query = _unitOfWork.Branchs.GetQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(b =>
                        b.BranchName.ToLower().Contains(searchLower) ||
                        b.BranchLocation.ToLower().Contains(searchLower));
                }
                var totalCount = await _unitOfWork.Branchs.CountAsync(query);

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

                var branchs = await _unitOfWork.Branchs.GetPagedAsync(
                    query: query,
                    orderBy: null,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: "Halls"
                );
                var branchDtos = branchs.Select(b => new BranchDetailsResponseDto
                {
                    BranchName = b.BranchName,
                    BranchLocation = b.BranchLocation
                }).ToList();
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

        public async Task<BranchDetailsResponseDto?> GetBranchByIdAsync(int branchId)
        {
            try
            {
                _logger.LogInformation("Getting Branch with branch Id {branchId}", branchId);
                if (branchId <= 0)
                {
                    _logger.LogWarning("Invalid branch Id {branchId}", branchId);
                    throw new ArgumentException("Invalid branch Id.");
                }
                var branch = await _unitOfWork.Branchs.GetByIdAsync(branchId);
                if (branch == null)
                {
                    _logger.LogWarning("Branch with Id {branchId} not found", branchId);
                    return null;
                }
                var response = new BranchDetailsResponseDto
                {
                    BranchName = branch.BranchName,
                    BranchLocation = branch.BranchLocation
                };
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
