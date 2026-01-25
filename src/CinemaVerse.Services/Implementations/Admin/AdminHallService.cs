using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminHallService : IAdminHallService
    {
        private readonly ILogger<AdminHallService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public AdminHallService(ILogger<AdminHallService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task<int> CreateHallAsync(CreateHallRequestDto Request)
        {
            try
            {
                _logger.LogInformation("Creating a new hall with name: {HallNumber}", Request.HallNumber);
                if (Request == null)
                {
                    _logger.LogWarning("CreateHallRequestDto is null.");
                    throw new ArgumentNullException(nameof(Request), "CreateHallRequestDto cannot be null.");
                }
                var branch = await _unitOfWork.Branchs.GetByIdAsync(Request.BranchId);
                if (branch == null)
                {
                    _logger.LogWarning("Branch with ID {BranchId} not found.", Request.BranchId);
                    throw new KeyNotFoundException($"Branch with ID {Request.BranchId} not found.");
                }
                var existingHall = await _unitOfWork.Halls.FirstOrDefaultAsync(h => h.HallNumber == Request.HallNumber && h.BranchId == Request.BranchId);
                if (existingHall != null)
                {
                    _logger.LogWarning("A hall with number {HallNumber} already exists in branch {BranchId}.",
                        Request.HallNumber, Request.BranchId);
                    throw new InvalidOperationException(
                        $"A hall with number {Request.HallNumber} already exists in this branch.");
                }
                var newHall = new Hall
                {
                    BranchId = Request.BranchId,
                    Capacity = Request.Capacity,
                    HallNumber = Request.HallNumber,
                    HallStatus = Request.HallStatus,
                    HallType = Request.HallType
                };
                await _unitOfWork.Halls.AddAsync(newHall);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully created a new hall with ID: {HallId}", newHall.Id);
                return newHall.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new hall.");
                throw;
            }
        }

        public async Task DeleteHallAsync(int hallId)
        {
            try
            {
                _logger.LogInformation("Deleting hall with id: {hallId}", hallId);
                if (hallId <= 0)
                {
                    _logger.LogWarning("Invalid hallId: {hallId}", hallId);
                    throw new ArgumentException("hallId must be greater than zero.", nameof(hallId));
                }
                var hall = await _unitOfWork.Halls.GetByIdAsync(hallId);
                if (hall == null)
                {
                    _logger.LogWarning("Hall with id {hallId} not found.", hallId);
                    throw new KeyNotFoundException($"Hall with id {hallId} not found.");
                }
                await _unitOfWork.Halls.DeleteAsync(hall);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted hall with id: {hallId}", hallId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting hall with id {hallId}.", hallId);
                throw;
            }
        }

        public async Task<int> EditHallAsync(int hallId, UpdateHallRequestDto Request)
        {
            try
            {
                _logger.LogInformation("Editing hall with id: {hallId}", hallId);
                if (Request == null)
                {
                    _logger.LogWarning("UpdateHallRequestDto is null.");
                    throw new ArgumentNullException(nameof(Request), "UpdateHallRequestDto cannot be null.");
                }
                if (hallId <= 0)
                {
                    _logger.LogWarning("Invalid hallId: {hallId}", hallId);
                    throw new ArgumentException("hallId must be greater than zero.", nameof(hallId));
                }
                var hall = await _unitOfWork.Halls.GetByIdAsync(hallId);
                if (hall == null)
                {
                    _logger.LogWarning("Hall with id {hallId} not found.", hallId);
                    throw new KeyNotFoundException($"Hall with id {hallId} not found.");
                }
                if (Request.BranchId.HasValue && Request.BranchId.Value != hall.BranchId)
                {
                    var branch = await _unitOfWork.Branchs.GetByIdAsync(Request.BranchId.Value);
                    if (branch == null)
                    {
                        _logger.LogWarning("Branch with ID {BranchId} not found.", Request.BranchId.Value);
                        throw new KeyNotFoundException($"Branch with ID {Request.BranchId.Value} not found.");
                    }
                    hall.BranchId = Request.BranchId.Value;
                }

                if (Request.Capacity.HasValue)
                    hall.Capacity = Request.Capacity.Value;

                if (!string.IsNullOrWhiteSpace(Request.HallNumber))
                    hall.HallNumber = Request.HallNumber;

                if (Request.HallStatus.HasValue)
                    hall.HallStatus = Request.HallStatus.Value;

                if (Request.HallType.HasValue)
                    hall.HallType = Request.HallType.Value;

                await _unitOfWork.Halls.UpdateAsync(hall);
                var result = await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully edited hall with id: {hallId}", hallId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Editing hall with id {hallId}.", hallId);
                throw;
            }
        }

        public async Task<PagedResultDto<HallDetailsResponseDto>> GetAllHallesAsync(AdminHallFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting all halls.");
                if (filter == null)
                {
                    _logger.LogWarning("AdminHallFilterDto is null.");
                    throw new ArgumentNullException(nameof(filter), "AdminHallFilterDto cannot be null.");
                }
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > 100)
                    filter.PageSize = 20;

                var query = _unitOfWork.Halls.GetQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(h =>h.HallNumber.ToLower().Contains(searchLower));
                }

                var totalCount = await _unitOfWork.Halls.CountAsync(query);

                string sortBy = filter.SortBy?.ToLower() ?? "hallnumber";
                string sortOrder = filter.SortOrder?.ToLower() ?? "desc";

                if (sortBy == "hallnumber")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.HallNumber)
                        : query.OrderByDescending(b => b.HallNumber);
                }
                else if (sortBy == "hallstatus")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.HallStatus)
                        : query.OrderByDescending(b => b.HallStatus);
                }
                else if (sortBy == "halltype")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.HallType)
                        : query.OrderByDescending(b => b.HallType);
                }
                else if (sortBy == "capacity")
                {
                    query = sortOrder == "asc"
                        ? query.OrderBy(b => b.Capacity)
                        : query.OrderByDescending(b => b.Capacity);
                }
                else // Default
                {
                    query = query.OrderBy(h => h.HallNumber);

                }

                var branchs = await _unitOfWork.Halls.GetPagedAsync(
                    query: query,
                    orderBy: null,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: "Branch"
                );
               
                var hallDtos = branchs.Select(hall => new HallDetailsResponseDto
                {
                    BranchId = hall.BranchId,
                    Capacity = hall.Capacity,
                    HallNumber = hall.HallNumber,
                    HallStatus = hall.HallStatus.ToString(),
                    HallType = hall.HallType.ToString()
                }).ToList();
                var pagedResult = new PagedResultDto<HallDetailsResponseDto>
                {
                    Items = hallDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };
                _logger.LogInformation("Successfully retrieved all halls.");
                return pagedResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all halls.");
                throw;
            }
        }

        public async Task<HallDetailsResponseDto?> GetHallByIdAsync(int hallId)
        {
            try
            {
                _logger.LogInformation("Getting hall with id: {hallId}", hallId);
                if (hallId <= 0)
                {
                    _logger.LogWarning("Invalid hallId: {hallId}", hallId);
                    throw new ArgumentException("hallId must be greater than zero.", nameof(hallId));
                }

                var hall = await _unitOfWork.Halls.GetByIdAsync(hallId);
                if (hall == null)
                {
                    _logger.LogWarning("Hall with id {hallId} not found.", hallId);
                    return null;
                }
                var hallDetails = new HallDetailsResponseDto
                {
                    BranchId = hall.BranchId,
                    Capacity = hall.Capacity,
                    HallNumber = hall.HallNumber,
                    HallStatus = hall.HallStatus.ToString(),
                    HallType = hall.HallType.ToString()
                };
                _logger.LogInformation("Successfully retrieved hall with id: {hallId}", hallId);
                return hallDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting hall with id {hallId}.", hallId);
                throw;
            }
        }
    }
}
