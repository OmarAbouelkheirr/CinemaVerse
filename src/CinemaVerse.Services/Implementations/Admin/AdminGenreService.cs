using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.AdminFlow.AdminGenre.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminGenre.Response;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.Interfaces.Admin;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminGenreService : IAdminGenreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminGenreService> _logger;

        public AdminGenreService(IUnitOfWork unitOfWork, ILogger<AdminGenreService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> CreateGenreAsync(CreateGenreRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating genre with name: {GenreName}", request.GenreName);

                if (request == null)
                {
                    _logger.LogWarning("CreateGenreRequestDto is null");
                    throw new ArgumentNullException(nameof(request), "CreateGenreRequestDto cannot be null");
                }

                // Check if genre name already exists
                var existingGenre = await _unitOfWork.Genres
                    .FirstOrDefaultAsync(g => g.GenreName.ToLower() == request.GenreName.ToLower());

                if (existingGenre != null)
                {
                    _logger.LogWarning("Genre with name {GenreName} already exists", request.GenreName);
                    throw new InvalidOperationException($"Genre with name '{request.GenreName}' already exists.");
                }

                var genre = new Genre
                {
                    GenreName = request.GenreName
                };

                await _unitOfWork.Genres.AddAsync(genre);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully created genre {GenreId} with name {GenreName}", 
                    genre.Id, genre.GenreName);

                return genre.Id;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating genre with name: {GenreName}", request?.GenreName);
                throw;
            }
        }

        public async Task<int> UpdateGenreAsync(int genreId, UpdateGenreRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Updating genre with ID: {GenreId}", genreId);

                if (request == null)
                {
                    _logger.LogWarning("UpdateGenreRequestDto is null");
                    throw new ArgumentNullException(nameof(request), "UpdateGenreRequestDto cannot be null");
                }

                if (genreId <= 0)
                {
                    _logger.LogWarning("Invalid genre ID: {GenreId}", genreId);
                    throw new ArgumentException("Genre ID must be a positive integer.", nameof(genreId));
                }

                var genre = await _unitOfWork.Genres.GetByIdAsync(genreId);
                if (genre == null)
                {
                    _logger.LogWarning("Genre with ID {GenreId} not found", genreId);
                    throw new KeyNotFoundException($"Genre with ID {genreId} not found.");
                }

                // Check for genre name conflict if being changed
                if (!string.IsNullOrWhiteSpace(request.GenreName))
                {
                    var existingGenre = await _unitOfWork.Genres
                        .FirstOrDefaultAsync(g => g.GenreName.ToLower() == request.GenreName.ToLower() &&
                                                  g.Id != genreId);

                    if (existingGenre != null)
                    {
                        _logger.LogWarning("Genre with name {GenreName} already exists", request.GenreName);
                        throw new InvalidOperationException(
                            $"Genre with name '{request.GenreName}' already exists.");
                    }
                }

                // Update properties (PATCH style)
                if (!string.IsNullOrWhiteSpace(request.GenreName))
                    genre.GenreName = request.GenreName;

                await _unitOfWork.Genres.UpdateAsync(genre);
                var result = await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully updated genre {GenreId}", genreId);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating genre with ID: {GenreId}", genreId);
                throw;
            }
        }

        public async Task DeleteGenreAsync(int genreId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (genreId <= 0)
                {
                    _logger.LogWarning("Invalid genre ID: {GenreId}", genreId);
                    throw new ArgumentException("Genre ID must be a positive integer.", nameof(genreId));
                }

                _logger.LogInformation("Deleting genre with ID: {GenreId}", genreId);

                var genre = await _unitOfWork.Genres.GetByIdAsync(genreId);
                if (genre == null)
                {
                    _logger.LogWarning("Genre with ID {GenreId} not found", genreId);
                    throw new KeyNotFoundException($"Genre with ID {genreId} not found.");
                }

                // Check if genre is used in any movies
                var hasMovies = await _unitOfWork.MovieGenres
                    .AnyAsync(mg => mg.GenreID == genreId);

                if (hasMovies)
                {
                    _logger.LogWarning("Cannot delete genre {GenreId} because it has associated movies", genreId);
                    throw new InvalidOperationException(
                        $"Cannot delete genre {genreId} because it has associated movies. Please remove genre from movies first.");
                }

                await _unitOfWork.Genres.DeleteAsync(genre);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully deleted genre {GenreId}", genreId);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deleting genre with ID: {GenreId}", genreId);
                throw;
            }
        }

        public async Task<GenreDetailsDto?> GetGenreAsync(int genreId)
        {
            try
            {
                _logger.LogInformation("Getting genre with ID: {GenreId}", genreId);

                if (genreId <= 0)
                {
                    _logger.LogWarning("Invalid genre ID: {GenreId}", genreId);
                    throw new ArgumentException("Genre ID must be a positive integer.", nameof(genreId));
                }

                var genre = await _unitOfWork.Genres.GetByIdAsync(genreId);
                if (genre == null)
                {
                    _logger.LogWarning("Genre with ID {GenreId} not found", genreId);
                    return null;
                }

                // Count movies associated with this genre
                var moviesCount = await _unitOfWork.MovieGenres
                    .CountAsync(mg => mg.GenreID == genreId);

                var dto = new GenreDetailsDto
                {
                    GenreId = genre.Id,
                    GenreName = genre.GenreName,
                    MoviesCount = moviesCount
                };

                _logger.LogInformation("Successfully retrieved genre {GenreId}: {GenreName}", genreId, genre.GenreName);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting genre with ID: {GenreId}", genreId);
                throw;
            }
        }

        public async Task<PagedResultDto<GenreDetailsDto>> GetAllGenresAsync(AdminGenreFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting all genres with filter: {@Filter}", filter);

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
                var query = _unitOfWork.Genres.GetQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(g => g.GenreName.ToLower().Contains(searchLower));
                }

                // Get total count before pagination
                var totalCount = await _unitOfWork.Genres.CountAsync(query);

                // Build orderBy function
                string sortBy = filter.SortBy?.ToLower() ?? "genrename";
                string sortOrder = filter.SortOrder?.ToLower() ?? "asc";

                Func<IQueryable<Genre>, IOrderedQueryable<Genre>> orderByFunc = sortBy switch
                {
                    "id" => sortOrder == "asc"
                        ? q => q.OrderBy(g => g.Id)
                        : q => q.OrderByDescending(g => g.Id),
                    _ => sortOrder == "asc"
                        ? q => q.OrderBy(g => g.GenreName)
                        : q => q.OrderByDescending(g => g.GenreName)
                };

                // Apply pagination
                var genres = await _unitOfWork.Genres.GetPagedAsync(
                    query: query,
                    orderBy: orderByFunc,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: null
                );

                // Get movie counts for each genre
                var genreIds = genres.Select(g => g.Id).ToList();
                var movieCounts = await _unitOfWork.MovieGenres
                    .FindAllAsync(mg => genreIds.Contains(mg.GenreID));

                var movieCountsDict = movieCounts
                    .GroupBy(mg => mg.GenreID)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Map to DTOs
                var genreDtos = genres.Select(genre => new GenreDetailsDto
                {
                    GenreId = genre.Id,
                    GenreName = genre.GenreName,
                    MoviesCount = movieCountsDict.GetValueOrDefault(genre.Id, 0)
                }).ToList();

                _logger.LogInformation("Retrieved {Count} genres out of {Total} total",
                    genreDtos.Count, totalCount);

                return new PagedResultDto<GenreDetailsDto>
                {
                    Items = genreDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all genres");
                throw;
            }
        }
    }
}
