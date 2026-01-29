using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.AdminFlow.AdminShowtime.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminShowtime.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminShowtimeService : IAdminShowtimeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminShowtimeService> _logger;

        public AdminShowtimeService(IUnitOfWork unitOfWork, ILogger<AdminShowtimeService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> CreateShowtimeAsync(CreateShowtimeRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating showtime for MovieId: {MovieId}, HallId: {HallId}", request.MovieId, request.HallId);

                if (request == null)
                {
                    _logger.LogWarning("CreateShowtimeRequestDto is null");
                    throw new ArgumentNullException(nameof(request), "CreateShowtimeRequestDto cannot be null");
                }

                // Validate Movie exists and is Active
                var movie = await _unitOfWork.Movies.GetByIdAsync(request.MovieId);
                if (movie == null)
                {
                    _logger.LogWarning("Movie with ID {MovieId} not found", request.MovieId);
                    throw new KeyNotFoundException($"Movie with ID {request.MovieId} not found.");
                }

                if (movie.Status != MovieStatus.Active)
                {
                    _logger.LogWarning("Movie with ID {MovieId} is not Active (Status: {Status})", request.MovieId, movie.Status);
                    throw new InvalidOperationException($"Cannot create showtime for movie with status {movie.Status}. Movie must be Active.");
                }

                // Validate Hall exists and is Available
                var hall = await _unitOfWork.Halls.GetByIdAsync(request.HallId);
                if (hall == null)
                {
                    _logger.LogWarning("Hall with ID {HallId} not found", request.HallId);
                    throw new KeyNotFoundException($"Hall with ID {request.HallId} not found.");
                }

                if (hall.HallStatus != HallStatus.Available)
                {
                    _logger.LogWarning("Hall with ID {HallId} is not Available (Status: {Status})", request.HallId, hall.HallStatus);
                    throw new InvalidOperationException($"Cannot create showtime for hall with status {hall.HallStatus}. Hall must be Available.");
                }

                // Validate BranchId if provided - check that Hall belongs to this Branch
                if (request.BranchId.HasValue)
                {
                    var branch = await _unitOfWork.Branches.GetByIdAsync(request.BranchId.Value);
                    if (branch == null)
                    {
                        _logger.LogWarning("Branch with ID {BranchId} not found", request.BranchId.Value);
                        throw new KeyNotFoundException($"Branch with ID {request.BranchId.Value} not found.");
                    }

                    if (hall.BranchId != request.BranchId.Value)
                    {
                        _logger.LogWarning("Hall {HallId} does not belong to Branch {BranchId}. Hall belongs to Branch {HallBranchId}",
                            request.HallId, request.BranchId.Value, hall.BranchId);
                        throw new ArgumentException($"Hall {hall.HallNumber} does not belong to Branch {branch.BranchName}. Please select a hall from the correct branch.");
                    }
                }

                // Calculate ShowEndTime based on MovieDuration
                var showEndTime = request.ShowStartTime.Add(movie.MovieDuration);

                // Check for time conflicts - no overlapping showtimes in the same hall
                var conflictingShowtime = await _unitOfWork.MovieShowTimes.FirstOrDefaultAsync(mst =>
                    mst.HallId == request.HallId &&
                    ((mst.ShowStartTime <= request.ShowStartTime && mst.ShowEndTime > request.ShowStartTime) ||
                     (mst.ShowStartTime < showEndTime && mst.ShowEndTime >= showEndTime) ||
                     (mst.ShowStartTime >= request.ShowStartTime && mst.ShowEndTime <= showEndTime)));

                if (conflictingShowtime != null)
                {
                    _logger.LogWarning("Time conflict detected: Hall {HallId} already has a showtime from {StartTime} to {EndTime}",
                        request.HallId, conflictingShowtime.ShowStartTime, conflictingShowtime.ShowEndTime);
                    throw new InvalidOperationException(
                        $"Hall {hall.HallNumber} already has a showtime scheduled from {conflictingShowtime.ShowStartTime:yyyy-MM-dd HH:mm} to {conflictingShowtime.ShowEndTime:yyyy-MM-dd HH:mm}. " +
                        "Cannot create overlapping showtimes in the same hall.");
                }

                // Check if ShowStartTime is in the future
                if (request.ShowStartTime <= DateTime.UtcNow)
                {
                    _logger.LogWarning("ShowStartTime {ShowStartTime} is in the past", request.ShowStartTime);
                    throw new ArgumentException("ShowStartTime must be in the future.");
                }

                var showtime = new MovieShowTime
                {
                    MovieId = request.MovieId,
                    HallId = request.HallId,
                    ShowStartTime = request.ShowStartTime,
                    ShowEndTime = showEndTime,
                    Price = request.Price,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.MovieShowTimes.AddAsync(showtime);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully created showtime with ID: {ShowtimeId}", showtime.Id);
                return showtime.Id;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating showtime for MovieId: {MovieId}, HallId: {HallId}", request.MovieId, request.HallId);
                throw;
            }
        }

        public async Task<int> UpdateShowtimeAsync(int showtimeId, UpdateShowtimeRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Updating showtime with ID: {ShowtimeId}", showtimeId);

                if (request == null)
                {
                    _logger.LogWarning("UpdateShowtimeRequestDto is null");
                    throw new ArgumentNullException(nameof(request), "UpdateShowtimeRequestDto cannot be null");
                }

                if (showtimeId <= 0)
                {
                    _logger.LogWarning("Invalid showtime ID: {ShowtimeId}", showtimeId);
                    throw new ArgumentException("Showtime ID must be a positive integer.", nameof(showtimeId));
                }

                var showtime = await _unitOfWork.MovieShowTimes.GetMovieShowTimeWithDetailsAsync(showtimeId);
                if (showtime == null)
                {
                    _logger.LogWarning("Showtime with ID {ShowtimeId} not found", showtimeId);
                    throw new KeyNotFoundException($"Showtime with ID {showtimeId} not found.");
                }

                // Check if showtime has bookings - if yes, only allow price updates
                var hasBookings = showtime.Bookings != null && showtime.Bookings.Any();
                if (hasBookings && (request.MovieId.HasValue || request.HallId.HasValue || request.ShowStartTime.HasValue))
                {
                    _logger.LogWarning("Cannot change MovieId, HallId, or ShowStartTime for showtime {ShowtimeId} that has bookings", showtimeId);
                    throw new InvalidOperationException("Cannot modify MovieId, HallId, or ShowStartTime for a showtime that has existing bookings. Only price can be updated.");
                }

                // Validate Movie if being changed
                Movie? movie = null;
                if (request.MovieId.HasValue && request.MovieId.Value != showtime.MovieId)
                {
                    movie = await _unitOfWork.Movies.GetByIdAsync(request.MovieId.Value);
                    if (movie == null)
                    {
                        _logger.LogWarning("Movie with ID {MovieId} not found", request.MovieId.Value);
                        throw new KeyNotFoundException($"Movie with ID {request.MovieId.Value} not found.");
                    }

                    if (movie.Status != MovieStatus.Active)
                    {
                        _logger.LogWarning("Movie with ID {MovieId} is not Active (Status: {Status})", request.MovieId.Value, movie.Status);
                        throw new InvalidOperationException($"Cannot update showtime to movie with status {movie.Status}. Movie must be Active.");
                    }

                    showtime.MovieId = request.MovieId.Value;
                }

                // Validate Hall if being changed
                if (request.HallId.HasValue && request.HallId.Value != showtime.HallId)
                {
                    var hall = await _unitOfWork.Halls.GetByIdAsync(request.HallId.Value);
                    if (hall == null)
                    {
                        _logger.LogWarning("Hall with ID {HallId} not found", request.HallId.Value);
                        throw new KeyNotFoundException($"Hall with ID {request.HallId.Value} not found.");
                    }

                    if (hall.HallStatus != HallStatus.Available)
                    {
                        _logger.LogWarning("Hall with ID {HallId} is not Available (Status: {Status})", request.HallId.Value, hall.HallStatus);
                        throw new InvalidOperationException($"Cannot update showtime to hall with status {hall.HallStatus}. Hall must be Available.");
                    }

                    showtime.HallId = request.HallId.Value;
                }

                // Update ShowStartTime if provided
                DateTime newShowStartTime = showtime.ShowStartTime;
                if (request.ShowStartTime.HasValue)
                {
                    if (request.ShowStartTime.Value <= DateTime.UtcNow)
                    {
                        _logger.LogWarning("ShowStartTime {ShowStartTime} is in the past", request.ShowStartTime.Value);
                        throw new ArgumentException("ShowStartTime must be in the future.");
                    }

                    newShowStartTime = request.ShowStartTime.Value;
                    showtime.ShowStartTime = newShowStartTime;
                }

                // Calculate ShowEndTime based on MovieDuration
                // Use movie from validation if available, otherwise load it
                if (movie == null)
                {
                    movie = await _unitOfWork.Movies.GetByIdAsync(showtime.MovieId);
                    if (movie == null)
                    {
                        _logger.LogWarning("Movie with ID {MovieId} not found for showtime {ShowtimeId}", showtime.MovieId, showtimeId);
                        throw new KeyNotFoundException($"Movie with ID {showtime.MovieId} not found.");
                    }
                }

                var newShowEndTime = newShowStartTime.Add(movie.MovieDuration);
                showtime.ShowEndTime = newShowEndTime;

                // Check for time conflicts if time or hall changed
                if (request.ShowStartTime.HasValue || request.HallId.HasValue)
                {
                    var conflictingShowtime = await _unitOfWork.MovieShowTimes.FirstOrDefaultAsync(mst =>
                        mst.HallId == showtime.HallId &&
                        mst.Id != showtimeId &&
                        ((mst.ShowStartTime <= showtime.ShowStartTime && mst.ShowEndTime > showtime.ShowStartTime) ||
                         (mst.ShowStartTime < showtime.ShowEndTime && mst.ShowEndTime >= showtime.ShowEndTime) ||
                         (mst.ShowStartTime >= showtime.ShowStartTime && mst.ShowEndTime <= showtime.ShowEndTime)));

                    if (conflictingShowtime != null)
                    {
                        _logger.LogWarning("Time conflict detected: Hall {HallId} already has a showtime from {StartTime} to {EndTime}",
                            showtime.HallId, conflictingShowtime.ShowStartTime, conflictingShowtime.ShowEndTime);
                        throw new InvalidOperationException(
                            $"Hall already has a showtime scheduled from {conflictingShowtime.ShowStartTime:yyyy-MM-dd HH:mm} to {conflictingShowtime.ShowEndTime:yyyy-MM-dd HH:mm}. " +
                            "Cannot create overlapping showtimes in the same hall.");
                    }
                }

                // Update Price if provided
                if (request.Price.HasValue)
                {
                    showtime.Price = request.Price.Value;
                }

                await _unitOfWork.MovieShowTimes.UpdateAsync(showtime);
                var result = await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully updated showtime with ID: {ShowtimeId}", showtimeId);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating showtime with ID: {ShowtimeId}", showtimeId);
                throw;
            }
        }

        public async Task<bool> DeleteShowtimeAsync(int showtimeId)
        {
            try
            {
                _logger.LogInformation("Deleting showtime with ID: {ShowtimeId}", showtimeId);

                if (showtimeId <= 0)
                {
                    _logger.LogWarning("Invalid showtime ID: {ShowtimeId}", showtimeId);
                    throw new ArgumentException("Showtime ID must be a positive integer.", nameof(showtimeId));
                }

                var showtime = await _unitOfWork.MovieShowTimes.GetMovieShowTimeWithDetailsAsync(showtimeId);
                if (showtime == null)
                {
                    _logger.LogWarning("Showtime with ID {ShowtimeId} not found", showtimeId);
                    throw new KeyNotFoundException($"Showtime with ID {showtimeId} not found.");
                }

                // Check if showtime has bookings
                if (showtime.Bookings != null && showtime.Bookings.Any())
                {
                    var bookingsCount = showtime.Bookings.Count;
                    _logger.LogWarning("Cannot delete showtime {ShowtimeId} that has {BookingsCount} bookings", showtimeId, bookingsCount);
                    throw new InvalidOperationException($"Cannot delete showtime with ID {showtimeId} because it has {bookingsCount} associated booking(s).");
                }

                await _unitOfWork.MovieShowTimes.DeleteAsync(showtime);
                var rowsAffected = await _unitOfWork.SaveChangesAsync();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Successfully deleted showtime with ID: {ShowtimeId}", showtimeId);
                    return true;
                }

                _logger.LogWarning("Delete operation affected 0 rows for showtime ID: {ShowtimeId}", showtimeId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting showtime with ID: {ShowtimeId}", showtimeId);
                throw;
            }
        }

        public async Task<ShowtimeDetailsDto?> GetShowtimeByIdAsync(int showtimeId)
        {
            try
            {
                _logger.LogInformation("Getting showtime with ID: {ShowtimeId}", showtimeId);

                if (showtimeId <= 0)
                {
                    _logger.LogWarning("Invalid showtime ID: {ShowtimeId}", showtimeId);
                    throw new ArgumentException("Showtime ID must be a positive integer.", nameof(showtimeId));
                }

                var showtime = await _unitOfWork.MovieShowTimes.GetMovieShowTimeWithDetailsAsync(showtimeId);
                if (showtime == null)
                {
                    _logger.LogWarning("Showtime with ID {ShowtimeId} not found", showtimeId);
                    return null;
                }

                var dto = new ShowtimeDetailsDto
                {
                    Id = showtime.Id,
                    MovieId = showtime.MovieId,
                    MovieName = showtime.Movie?.MovieName ?? string.Empty,
                    HallId = showtime.HallId,
                    HallNumber = showtime.Hall?.HallNumber ?? string.Empty,
                    BranchId = showtime.Hall?.BranchId ?? 0,
                    BranchName = showtime.Hall?.Branch?.BranchName ?? string.Empty,
                    ShowStartTime = showtime.ShowStartTime,
                    ShowEndTime = showtime.ShowEndTime,
                    Price = showtime.Price,
                    CreatedAt = showtime.CreatedAt,
                    TotalBookings = showtime.Bookings?.Count ?? 0,
                    TotalTickets = showtime.Bookings?.SelectMany(b => b.Tickets ?? Enumerable.Empty<Ticket>()).Count() ?? 0
                };

                _logger.LogInformation("Successfully retrieved showtime with ID: {ShowtimeId}", showtimeId);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting showtime with ID: {ShowtimeId}", showtimeId);
                throw;
            }
        }

        public async Task<PagedResultDto<ShowtimeDetailsDto>> GetAllShowtimesAsync(AdminShowtimeFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting all showtimes with filter: {@Filter}", filter);

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
                var query = _unitOfWork.MovieShowTimes.GetQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(mst =>
                        (mst.Movie != null && mst.Movie.MovieName.ToLower().Contains(searchLower)) ||
                        (mst.Hall != null && mst.Hall.HallNumber.ToLower().Contains(searchLower)));
                }

                if (filter.MovieId.HasValue)
                {
                    query = query.Where(mst => mst.MovieId == filter.MovieId.Value);
                }

                if (filter.HallId.HasValue)
                {
                    query = query.Where(mst => mst.HallId == filter.HallId.Value);
                }

                if (filter.BranchId.HasValue)
                {
                    query = query.Where(mst => mst.Hall != null && mst.Hall.BranchId == filter.BranchId.Value);
                }

                if (filter.DateFrom.HasValue)
                {
                    query = query.Where(mst => mst.ShowStartTime >= filter.DateFrom.Value);
                }

                if (filter.DateTo.HasValue)
                {
                    query = query.Where(mst => mst.ShowStartTime <= filter.DateTo.Value);
                }

                if (filter.PriceMin.HasValue)
                {
                    query = query.Where(mst => mst.Price >= filter.PriceMin.Value);
                }

                if (filter.PriceMax.HasValue)
                {
                    query = query.Where(mst => mst.Price <= filter.PriceMax.Value);
                }

                // Get total count before pagination
                var totalCount = await _unitOfWork.MovieShowTimes.CountAsync(query);

                // Build orderBy function
                string sortBy = filter.SortBy?.ToLower() ?? "showstarttime";
                string sortOrder = filter.SortOrder?.ToLower() ?? "desc";

                Func<IQueryable<MovieShowTime>, IOrderedQueryable<MovieShowTime>> orderByFunc = sortBy switch
                {
                    "price" => sortOrder == "asc"
                        ? q => q.OrderBy(mst => mst.Price)
                        : q => q.OrderByDescending(mst => mst.Price),
                    "moviename" => sortOrder == "asc"
                        ? q => q.OrderBy(mst => mst.Movie != null ? mst.Movie.MovieName : string.Empty)
                        : q => q.OrderByDescending(mst => mst.Movie != null ? mst.Movie.MovieName : string.Empty),
                    "hallnumber" => sortOrder == "asc"
                        ? q => q.OrderBy(mst => mst.Hall != null ? mst.Hall.HallNumber : string.Empty)
                        : q => q.OrderByDescending(mst => mst.Hall != null ? mst.Hall.HallNumber : string.Empty),
                    _ => sortOrder == "asc"
                        ? q => q.OrderBy(mst => mst.ShowStartTime)
                        : q => q.OrderByDescending(mst => mst.ShowStartTime)
                };

                var showtimes = await _unitOfWork.MovieShowTimes.GetPagedAsync(
                    query: query,
                    orderBy: orderByFunc,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: "Movie,Hall.Branch,Bookings.Tickets"
                );

                var showtimeDtos = showtimes.Select(mst => new ShowtimeDetailsDto
                {
                    Id = mst.Id,
                    MovieId = mst.MovieId,
                    MovieName = mst.Movie?.MovieName ?? string.Empty,
                    HallId = mst.HallId,
                    HallNumber = mst.Hall?.HallNumber ?? string.Empty,
                    BranchId = mst.Hall?.BranchId ?? 0,
                    BranchName = mst.Hall?.Branch?.BranchName ?? string.Empty,
                    ShowStartTime = mst.ShowStartTime,
                    ShowEndTime = mst.ShowEndTime,
                    Price = mst.Price,
                    CreatedAt = mst.CreatedAt,
                    TotalBookings = mst.Bookings?.Count ?? 0,
                    TotalTickets = mst.Bookings?.SelectMany(b => b.Tickets ?? Enumerable.Empty<Ticket>()).Count() ?? 0
                }).ToList();

                _logger.LogInformation("Retrieved {Count} showtimes out of {Total} total",
                    showtimeDtos.Count, totalCount);

                return new PagedResultDto<ShowtimeDetailsDto>
                {
                    Items = showtimeDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all showtimes");
                throw;
            }
        }
    }
}
