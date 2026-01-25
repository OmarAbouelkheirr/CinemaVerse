using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminSeatService : IAdminSeatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminSeatService> _logger;

        public AdminSeatService(IUnitOfWork unitOfWork, ILogger<AdminSeatService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> CreateSeatAsync(CreateSeatRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating seat with label: {SeatLabel} for HallId: {HallId}", 
                    request.SeatLabel, request.HallId);

                if (request == null)
                {
                    _logger.LogWarning("CreateSeatRequestDto is null");
                    throw new ArgumentNullException(nameof(request), "CreateSeatRequestDto cannot be null");
                }

                // Validate Hall exists
                var hall = await _unitOfWork.Halls.GetByIdAsync(request.HallId);
                if (hall == null)
                {
                    _logger.LogWarning("Hall with ID {HallId} not found", request.HallId);
                    throw new KeyNotFoundException($"Hall with ID {request.HallId} not found.");
                }

                // Check if seat label already exists in this hall
                var existingSeat = await _unitOfWork.Seats
                    .FirstOrDefaultAsync(s => s.HallId == request.HallId && 
                                              s.SeatLabel.ToLower() == request.SeatLabel.ToLower());

                if (existingSeat != null)
                {
                    _logger.LogWarning("Seat with label {SeatLabel} already exists in Hall {HallId}", 
                        request.SeatLabel, request.HallId);
                    throw new InvalidOperationException(
                        $"Seat with label '{request.SeatLabel}' already exists in Hall {request.HallId}.");
                }

                var seat = new Seat
                {
                    SeatLabel = request.SeatLabel,
                    HallId = request.HallId
                };

                await _unitOfWork.Seats.AddAsync(seat);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully created seat {SeatId} with label {SeatLabel} in Hall {HallId}", 
                    seat.Id, seat.SeatLabel, request.HallId);

                return seat.Id;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating seat with label: {SeatLabel} for HallId: {HallId}", 
                    request?.SeatLabel, request?.HallId);
                throw;
            }
        }

        public async Task<List<int>> CreateMultipleSeatsAsync(CreateMultipleSeatsRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating {Count} seats for HallId: {HallId}", 
                    request.SeatLabels.Count, request.HallId);

                if (request == null)
                {
                    _logger.LogWarning("CreateMultipleSeatsRequestDto is null");
                    throw new ArgumentNullException(nameof(request), "CreateMultipleSeatsRequestDto cannot be null");
                }

                // Validate Hall exists
                var hall = await _unitOfWork.Halls.GetByIdAsync(request.HallId);
                if (hall == null)
                {
                    _logger.LogWarning("Hall with ID {HallId} not found", request.HallId);
                    throw new KeyNotFoundException($"Hall with ID {request.HallId} not found.");
                }

                // Get existing seats in this hall
                var existingSeats = await _unitOfWork.Seats
                    .FindAllAsync(s => s.HallId == request.HallId);

                var existingLabels = existingSeats
                    .Select(s => s.SeatLabel.ToLower())
                    .ToHashSet();

                // Check for duplicates in request
                var duplicateLabels = request.SeatLabels
                    .GroupBy(l => l.ToLower())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateLabels.Any())
                {
                    _logger.LogWarning("Duplicate seat labels found: {Labels}", string.Join(", ", duplicateLabels));
                    throw new ArgumentException($"Duplicate seat labels found: {string.Join(", ", duplicateLabels)}");
                }

                // Check for conflicts with existing seats
                var conflictingLabels = request.SeatLabels
                    .Where(l => existingLabels.Contains(l.ToLower()))
                    .ToList();

                if (conflictingLabels.Any())
                {
                    _logger.LogWarning("Seat labels already exist in Hall {HallId}: {Labels}", 
                        request.HallId, string.Join(", ", conflictingLabels));
                    throw new InvalidOperationException(
                        $"Seat labels already exist in Hall {request.HallId}: {string.Join(", ", conflictingLabels)}");
                }

                var createdSeatIds = new List<int>();

                foreach (var seatLabel in request.SeatLabels)
                {
                    var seat = new Seat
                    {
                        SeatLabel = seatLabel,
                        HallId = request.HallId
                    };

                    await _unitOfWork.Seats.AddAsync(seat);
                    createdSeatIds.Add(seat.Id);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully created {Count} seats for HallId: {HallId}", 
                    createdSeatIds.Count, request.HallId);

                return createdSeatIds;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating multiple seats for HallId: {HallId}", request?.HallId);
                throw;
            }
        }

        public async Task<int> UpdateSeatAsync(int seatId, UpdateSeatRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Updating seat with ID: {SeatId}", seatId);

                if (request == null)
                {
                    _logger.LogWarning("UpdateSeatRequestDto is null");
                    throw new ArgumentNullException(nameof(request), "UpdateSeatRequestDto cannot be null");
                }

                if (seatId <= 0)
                {
                    _logger.LogWarning("Invalid seat ID: {SeatId}", seatId);
                    throw new ArgumentException("Seat ID must be a positive integer.", nameof(seatId));
                }

                var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
                if (seat == null)
                {
                    _logger.LogWarning("Seat with ID {SeatId} not found", seatId);
                    throw new KeyNotFoundException($"Seat with ID {seatId} not found.");
                }

                // Validate Hall if being changed
                if (request.HallId.HasValue)
                {
                    var hall = await _unitOfWork.Halls.GetByIdAsync(request.HallId.Value);
                    if (hall == null)
                    {
                        _logger.LogWarning("Hall with ID {HallId} not found", request.HallId.Value);
                        throw new KeyNotFoundException($"Hall with ID {request.HallId.Value} not found.");
                    }
                }

                // Check for seat label conflict if being changed
                if (!string.IsNullOrWhiteSpace(request.SeatLabel))
                {
                    var targetHallId = request.HallId ?? seat.HallId;
                    var existingSeat = await _unitOfWork.Seats
                        .FirstOrDefaultAsync(s => s.HallId == targetHallId && 
                                                  s.SeatLabel.ToLower() == request.SeatLabel.ToLower() &&
                                                  s.Id != seatId);

                    if (existingSeat != null)
                    {
                        _logger.LogWarning("Seat with label {SeatLabel} already exists in Hall {HallId}", 
                            request.SeatLabel, targetHallId);
                        throw new InvalidOperationException(
                            $"Seat with label '{request.SeatLabel}' already exists in Hall {targetHallId}.");
                    }
                }

                // Update properties (PATCH style)
                if (!string.IsNullOrWhiteSpace(request.SeatLabel))
                    seat.SeatLabel = request.SeatLabel;

                if (request.HallId.HasValue)
                    seat.HallId = request.HallId.Value;

                await _unitOfWork.Seats.UpdateAsync(seat);
                var result = await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully updated seat {SeatId}", seatId);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating seat with ID: {SeatId}", seatId);
                throw;
            }
        }

        public async Task DeleteSeatAsync(int seatId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (seatId <= 0)
                {
                    _logger.LogWarning("Invalid seat ID: {SeatId}", seatId);
                    throw new ArgumentException("Seat ID must be a positive integer.", nameof(seatId));
                }

                _logger.LogInformation("Deleting seat with ID: {SeatId}", seatId);

                var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
                if (seat == null)
                {
                    _logger.LogWarning("Seat with ID {SeatId} not found", seatId);
                    throw new KeyNotFoundException($"Seat with ID {seatId} not found.");
                }

                // Check if seat is used in any bookings/tickets
                var hasBookings = await _unitOfWork.BookingSeat
                    .AnyAsync(bs => bs.SeatId == seatId);

                if (hasBookings)
                {
                    _logger.LogWarning("Cannot delete seat {SeatId} because it has associated bookings", seatId);
                    throw new InvalidOperationException(
                        $"Cannot delete seat {seatId} because it has associated bookings. Please remove bookings first.");
                }

                await _unitOfWork.Seats.DeleteAsync(seat);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully deleted seat {SeatId}", seatId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deleting seat with ID: {SeatId}", seatId);
                throw;
            }
        }

        public async Task<SeatDetailsDto?> GetSeatAsync(int seatId)
        {
            try
            {
                _logger.LogInformation("Getting seat with ID: {SeatId}", seatId);

                if (seatId <= 0)
                {
                    _logger.LogWarning("Invalid seat ID: {SeatId}", seatId);
                    throw new ArgumentException("Seat ID must be a positive integer.", nameof(seatId));
                }

                var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
                if (seat == null)
                {
                    _logger.LogWarning("Seat with ID {SeatId} not found", seatId);
                    return null;
                }

                // Load Hall and Branch details
                var hall = await _unitOfWork.Halls.GetByIdAsync(seat.HallId);
                var branch = hall != null ? await _unitOfWork.Branchs.GetByIdAsync(hall.BranchId) : null;

                var dto = new SeatDetailsDto
                {
                    SeatId = seat.Id,
                    SeatLabel = seat.SeatLabel,
                    HallId = seat.HallId,
                    HallNumber = hall?.HallNumber ?? string.Empty,
                    BranchName = branch?.BranchName ?? string.Empty
                };

                _logger.LogInformation("Successfully retrieved seat {SeatId}: {SeatLabel}", seatId, seat.SeatLabel);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seat with ID: {SeatId}", seatId);
                throw;
            }
        }

        public async Task<PagedResultDto<SeatDetailsDto>> GetAllSeatsAsync(AdminSeatFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting all seats with filter: {@Filter}", filter);

                if (filter == null)
                {
                    throw new ArgumentNullException(nameof(filter));
                }

                // Validate pagination
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > 100)
                    filter.PageSize = 20;

                // Build query
                var query = _unitOfWork.Seats.GetQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(s => s.SeatLabel.ToLower().Contains(searchLower));
                }

                if (filter.HallId.HasValue)
                {
                    query = query.Where(s => s.HallId == filter.HallId.Value);
                }

                // Get total count before pagination
                var totalCount = await _unitOfWork.Seats.CountAsync(query);

                // Build orderBy function
                string sortBy = filter.SortBy?.ToLower() ?? "seatlabel";
                string sortOrder = filter.SortOrder?.ToLower() ?? "asc";

                Func<IQueryable<Seat>, IOrderedQueryable<Seat>> orderByFunc = sortBy switch
                {
                    "hallid" => sortOrder == "asc"
                        ? q => q.OrderBy(s => s.HallId).ThenBy(s => s.SeatLabel)
                        : q => q.OrderByDescending(s => s.HallId).ThenBy(s => s.SeatLabel),
                    _ => sortOrder == "asc"
                        ? q => q.OrderBy(s => s.SeatLabel)
                        : q => q.OrderByDescending(s => s.SeatLabel)
                };

                // Apply pagination
                var seats = await _unitOfWork.Seats.GetPagedAsync(
                    query: query,
                    orderBy: orderByFunc,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: "Hall.Branch"
                );

                // Map to DTOs
                var seatDtos = seats.Select(seat => new SeatDetailsDto
                {
                    SeatId = seat.Id,
                    SeatLabel = seat.SeatLabel,
                    HallId = seat.HallId,
                    HallNumber = seat.Hall?.HallNumber ?? string.Empty,
                    BranchName = seat.Hall?.Branch?.BranchName ?? string.Empty
                }).ToList();

                _logger.LogInformation("Retrieved {Count} seats out of {Total} total",
                    seatDtos.Count, totalCount);

                return new PagedResultDto<SeatDetailsDto>
                {
                    Items = seatDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all seats");
                throw;
            }
        }
    }
}
