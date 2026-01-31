using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using CinemaVerse.Services.Mappers;
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


        public async Task<SeatDetailsDto?> GetSeatAsync(int seatId)
        {
            try
            {
                _logger.LogInformation("Getting seat with ID: {SeatId}", seatId);

                if (seatId <= 0)
                {
                    throw new ArgumentException("Seat ID must be a positive integer.", nameof(seatId));
                }
                var seat = await _unitOfWork.Seats.GetByIdAsync(seatId);
                if (seat == null)
                    throw new KeyNotFoundException($"Seat with ID {seatId} not found.");

                // Load Hall and Branch details
                var hall = await _unitOfWork.Halls.GetByIdAsync(seat.HallId);
                var branch = hall != null ? await _unitOfWork.Branches.GetByIdAsync(hall.BranchId) : null;

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

                if (filter.PageSize <= 0 || filter.PageSize > PaginationConstants.MaxPageSize)
                    filter.PageSize = PaginationConstants.DefaultPageSize;

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
                var seatDtos = seats.Select(seat => SeatMapper.ToSeatDetailsDto(seat, seat.Hall)).ToList();

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
