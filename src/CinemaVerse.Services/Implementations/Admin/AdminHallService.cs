using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using CinemaVerse.Services.Mappers;
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

        public async Task<int> CreateHallAsync(CreateHallRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating a new hall with number: {HallNumber} and type: {HallType}",
                    request.HallNumber, request.HallType);

                var branch = await _unitOfWork.Branches.GetByIdAsync(request.BranchId);
                if (branch == null)
                    throw new KeyNotFoundException($"Branch with ID {request.BranchId} not found.");
                var hallNumberExists = await _unitOfWork.Halls.IsHallNumberExistsAsync(request.BranchId, request.HallNumber);
                if (hallNumberExists)
                    throw new InvalidOperationException($"A hall with number {request.HallNumber} already exists in this branch.");

                var capacity = HallTypeLayoutConfig.GetCapacity(request.HallType);
                var newHall = new Hall
                {
                    BranchId = request.BranchId,
                    Capacity = capacity,
                    HallNumber = request.HallNumber,
                    HallStatus = request.HallStatus,
                    HallType = request.HallType
                };

                // 1) Create hall
                await _unitOfWork.Halls.AddAsync(newHall);
                await _unitOfWork.SaveChangesAsync(); // Needed to get Hall.Id

                // 2) Generate seats for this hall based on HallType (fixed layout, full rows only)
                var seats = GenerateSeatsForHall(newHall.Id, newHall.HallType);

                foreach (var seat in seats)
                {
                    await _unitOfWork.Seats.AddAsync(seat);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Successfully created a new hall with ID: {HallId} and generated {SeatCount} seats (HallType: {HallType})",
                    newHall.Id, seats.Count, newHall.HallType);

                return newHall.Id;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error occurred while creating a new hall with automatic seats.");
                throw;
            }
        }

        private List<Seat> GenerateSeatsForHall(int hallId, HallType hallType)
        {
            var layout = HallTypeLayoutConfig.GetLayout(hallType);
            return GenerateSeatsFromLayout(hallId, layout);
        }

        private static List<Seat> GenerateSeatsFromLayout(int hallId, HallLayoutInfo layout)
        {
            var seats = new List<Seat>();
            if (layout.NumberOfRows <= 0 || layout.SeatColumns.Count == 0)
                return seats;

            char rowLetter = 'A';
            for (int row = 0; row < layout.NumberOfRows; row++)
            {
                foreach (var col in layout.SeatColumns)
                {
                    string seatLabel = $"{rowLetter}{col}";
                    seats.Add(new Seat
                    {
                        HallId = hallId,
                        SeatLabel = seatLabel
                    });
                }
                rowLetter++;
            }

            return seats;
        }

        public async Task DeleteHallAsync(int hallId)
        {
            try
            {
                _logger.LogInformation("Deleting hall with id: {hallId}", hallId);
                if (hallId <= 0)
                    throw new ArgumentException("Hall ID must be a positive integer.", nameof(hallId));
                var hall = await _unitOfWork.Halls.GetByIdAsync(hallId);
                if (hall == null)
                    throw new KeyNotFoundException($"Hall with id {hallId} not found.");
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

        public async Task<int> EditHallAsync(int hallId, UpdateHallRequestDto request)
        {
            try
            {
                _logger.LogInformation("Editing hall with id: {hallId}", hallId);
                if (hallId <= 0)
                    throw new ArgumentException("Hall ID must be a positive integer.", nameof(hallId));
                var hall = await _unitOfWork.Halls.GetByIdAsync(hallId);
                if (hall == null)
                    throw new KeyNotFoundException($"Hall with id {hallId} not found.");

                if (request.BranchId.HasValue && request.BranchId.Value != hall.BranchId)
                {
                    var branch = await _unitOfWork.Branches.GetByIdAsync(request.BranchId.Value);
                    if (branch == null)
                        throw new KeyNotFoundException($"Branch with ID {request.BranchId.Value} not found.");
                    hall.BranchId = request.BranchId.Value;
                }

                if (!string.IsNullOrWhiteSpace(request.HallNumber))
                    hall.HallNumber = request.HallNumber;

                if (request.HallStatus.HasValue)
                    hall.HallStatus = request.HallStatus.Value;

                var hallTypeChanged = request.HallType.HasValue && request.HallType.Value != hall.HallType;
                if (request.HallType.HasValue)
                    hall.HallType = request.HallType.Value;

                if (hallTypeChanged)
                {
                    await _unitOfWork.BeginTransactionAsync();
                    try
                    {
                        hall.Capacity = HallTypeLayoutConfig.GetCapacity(hall.HallType);
                        var existingSeats = (await _unitOfWork.Seats.FindAllAsync(s => s.HallId == hallId)).ToList();
                        foreach (var seat in existingSeats)
                            await _unitOfWork.Seats.DeleteAsync(seat);
                        await _unitOfWork.SaveChangesAsync();

                        var newSeats = GenerateSeatsForHall(hallId, hall.HallType);
                        foreach (var seat in newSeats)
                            await _unitOfWork.Seats.AddAsync(seat);

                        await _unitOfWork.Halls.UpdateAsync(hall);
                        var result = await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        _logger.LogInformation("Successfully edited hall with id: {hallId}, hall type changed; regenerated {SeatCount} seats.", hallId, newSeats.Count);
                        return result;
                    }
                    catch (Exception ex)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError(ex, "Error while updating hall type and regenerating seats for hall {hallId}.", hallId);
                        throw;
                    }
                }

                await _unitOfWork.Halls.UpdateAsync(hall);
                var saveResult = await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully edited hall with id: {hallId}", hallId);
                return saveResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Editing hall with id {hallId}.", hallId);
                throw;
            }
        }

        public async Task<PagedResultDto<HallDetailsResponseDto>> GetAllHallsAsync(AdminHallFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting all halls.");
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > PaginationConstants.MaxPageSize)
                    filter.PageSize = PaginationConstants.DefaultPageSize;

                var query = _unitOfWork.Halls.GetQueryable();

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(h => h.HallNumber.ToLower().Contains(searchLower));
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

                var halls = await _unitOfWork.Halls.GetPagedAsync(
                    query: query,
                    orderBy: null,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: "Branch"
                );

                var hallDtos = halls.Select(HallMapper.ToHallDetailsResponseDto).ToList();
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

        public async Task<HallDetailsResponseDto> GetHallWithSeatsByIdAsync(int hallId)
        {
            try
            {
                _logger.LogInformation("Getting hall with id: {hallId}", hallId);
                if (hallId <= 0)
                    throw new ArgumentException("Hall ID must be a positive integer.", nameof(hallId));
                var hall = await _unitOfWork.Halls.GetByIdWithDetailsAsync(hallId);
                if (hall == null)
                    throw new KeyNotFoundException($"Hall with id {hallId} not found.");
                var hallDetails = HallMapper.ToHallDetailsResponseDtoWithSeats(hall);
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
