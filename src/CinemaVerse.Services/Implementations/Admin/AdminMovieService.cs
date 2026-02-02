using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.AdminFlow.AdminMovie.Requests;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.UserFlow.Movie.Flow;
using CinemaVerse.Services.Interfaces.Admin;
using CinemaVerse.Services.Mappers;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.Admin
{
    public class AdminMovieService : IAdminMovieService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminMovieService> _logger;
        public AdminMovieService(IUnitOfWork unitOfWork, ILogger<AdminMovieService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<int> CreateMovieAsync(CreateMovieRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Creating movie with movie name : {MovieName}", request.MovieName);
                var existingMovie = await _unitOfWork.Movies
                        .FirstOrDefaultAsync(m => m.MovieName.ToLower() == request.MovieName.ToLower());

                if (existingMovie != null)
                    throw new InvalidOperationException($"Movie with name {request.MovieName} already exists.");

                // Validate all GenreIds exist - Single database call
                var allGenres = await _unitOfWork.Genres.GetAllAsync();
                var existingGenreIds = allGenres
                    .Where(g => request.GenreIds.Contains(g.Id))
                    .Select(g => g.Id)
                    .ToList(); 

                var invalidGenreIds = request.GenreIds.Except(existingGenreIds).ToList();
                if (invalidGenreIds.Any())
                    throw new ArgumentException($"Genres with IDs {string.Join(", ", invalidGenreIds)} do not exist.");

                var movie = new Movie
                {
                    MovieName = request.MovieName,
                    MovieDescription = request.MovieDescription,
                    MovieDuration = request.MovieDuration,
                    ReleaseDate = request.ReleaseDate,
                    MovieAgeRating = request.MovieAgeRating,
                    MovieRating = 0,
                    TrailerUrl = request.TrailerUrl!,
                    MoviePoster = request.MoviePoster ?? string.Empty,
                    Language = request.Language ?? string.Empty,
                    Status = request.Status
                };
                await _unitOfWork.Movies.AddAsync(movie);

                await _unitOfWork.SaveChangesAsync();

                // Add cast members after movie is saved
                foreach (var cast in request.CastMembers)
                {
                    await _unitOfWork.MovieCastMembers.AddAsync(new MovieCastMember
                    {
                        MovieId = movie.Id,
                        PersonName = cast.PersonName,
                        ImageUrl = cast.ImageUrl,
                        RoleType = cast.RoleType,
                        CharacterName = cast.CharacterName,
                        DisplayOrder = cast.DisplayOrder,
                        IsLead = cast.IsLead
                    });
                }

                // Add genres and images after movie is saved
                foreach (var genreId in request.GenreIds)
                {
                    await _unitOfWork.MovieGenres.AddAsync(new MovieGenre
                    {
                        MovieID = movie.Id,
                        GenreID = genreId
                    });
                }

                foreach (var imageUrl in request.ImageUrls)
                {
                    await _unitOfWork.MovieImages.AddAsync(new MovieImage
                    {
                        MovieId = movie.Id,
                        ImageUrl = imageUrl
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully created movie {MovieId}: {MovieName}", movie.Id, request.MovieName);
                return movie.Id;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating movie with movie name : {MovieName}", request.MovieName);
                throw;
            }
        }

        public async Task DeleteMovieAsync(int movieId)
        {
            try
            {
                if (movieId <= 0)
                    throw new ArgumentException("Movie ID must be a positive integer.", nameof(movieId));
                _logger.LogInformation("Deleting movie with ID: {MovieId}", movieId);
                var movie = await _unitOfWork.Movies.GetByIdAsync(movieId);
                if (movie == null)
                    throw new KeyNotFoundException($"Movie with ID {movieId} not found.");
                await _unitOfWork.Movies.DeleteAsync(movie);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting movie with ID: {MovieId}", movieId);
                throw;
            }
        }

        public async Task<int> EditMovieAsync(int movieId, UpdateMovieRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Updating movie with movie id {EditMovie}", movieId);
                if (movieId <= 0)
                    throw new ArgumentException("Movie ID must be a positive integer.", nameof(movieId));
                var movie = await _unitOfWork.Movies.GetByIdAsync(movieId);
                if (movie == null)
                    throw new KeyNotFoundException($"Movie with ID {movieId} not found.");

                // ✅ Validate GenreIds if provided - Single database call
                if (request.GenreIds != null && request.GenreIds.Any())
                {
                    var genreQuery = _unitOfWork.Genres.GetQueryable();
                    var existingGenreIds = genreQuery
                        .Where(g => request.GenreIds.Contains(g.Id))
                        .Select(g => g.Id)
                        .ToList();

                    var invalidGenreIds = request.GenreIds.Except(existingGenreIds).ToList();
                    if (invalidGenreIds.Any())
                        throw new ArgumentException($"Genres with IDs {string.Join(", ", invalidGenreIds)} do not exist.");
                }

                // ===== UPDATE MOVIE PROPERTIES (PATCH style) =====
                if (request.MovieName != null)
                    movie.MovieName = request.MovieName;

                if (request.MovieDescription != null)
                    movie.MovieDescription = request.MovieDescription;

                if (request.MovieDuration.HasValue)
                    movie.MovieDuration = request.MovieDuration.Value;

                if (request.ReleaseDate.HasValue)
                    movie.ReleaseDate = request.ReleaseDate.Value;

                // ===== UPDATE CAST MEMBERS (if provided) =====
                if (request.CastMembers != null)
                {
                    var existingCast = await _unitOfWork.MovieCastMembers
                        .FindAllAsync(c => c.MovieId == movieId);
                    foreach (var c in existingCast)
                    {
                        await _unitOfWork.MovieCastMembers.DeleteAsync(c);
                    }
                    foreach (var cast in request.CastMembers)
                    {
                        await _unitOfWork.MovieCastMembers.AddAsync(new MovieCastMember
                        {
                            MovieId = movieId,
                            PersonName = cast.PersonName,
                            ImageUrl = cast.ImageUrl,
                            RoleType = cast.RoleType,
                            CharacterName = cast.CharacterName,
                            DisplayOrder = cast.DisplayOrder,
                            IsLead = cast.IsLead
                        });
                    }
                }

                if (request.MovieAgeRating.HasValue)
                    movie.MovieAgeRating = request.MovieAgeRating.Value;

                if (request.TrailerUrl != null)
                    movie.TrailerUrl = request.TrailerUrl;

                if (request.MoviePoster != null)
                    movie.MoviePoster = request.MoviePoster;

                if (request.Language != null)
                    movie.Language = request.Language;

                // MovieRating is driven by user reviews only - do not update from Admin

                if (request.Status.HasValue)
                    movie.Status = request.Status.Value;

                await _unitOfWork.Movies.UpdateAsync(movie);

                // ===== UPDATE GENRES (if provided) =====
                if (request.GenreIds != null)
                {
                    // ✅ Remove existing genres - Bulk delete
                    var existingGenres = await _unitOfWork.MovieGenres
                        .FindAllAsync(mg => mg.MovieID == movieId);

                    // ✅ Fixed: Use loop instead of DeleteRangeAsync
                    foreach (var genre in existingGenres)
                    {
                        await _unitOfWork.MovieGenres.DeleteAsync(genre);
                    }

                    // Add new genres
                    foreach (var genreId in request.GenreIds)
                    {
                        await _unitOfWork.MovieGenres.AddAsync(new MovieGenre
                        {
                            MovieID = movieId,
                            GenreID = genreId
                        });
                    }
                }

                // ===== UPDATE IMAGES (if provided) =====
                if (request.ImageUrls != null)
                {
                    // ✅ Remove existing images - Bulk delete
                    var existingImages = await _unitOfWork.MovieImages
                        .FindAllAsync(mi => mi.MovieId == movieId);

                    // ✅ Fixed: Use loop instead of DeleteRangeAsync
                    foreach (var image in existingImages)
                    {
                        await _unitOfWork.MovieImages.DeleteAsync(image);
                    }

                    // Add new images
                    foreach (var imageUrl in request.ImageUrls)
                    {
                        await _unitOfWork.MovieImages.AddAsync(new MovieImage
                        {
                            MovieId = movieId,
                            ImageUrl = imageUrl
                        });
                    }
                }

                // ✅ Single SaveChanges with transaction
                var result = await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully updated movie {MovieId}", movieId);
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating movie with movie id {EditMovie}", movieId);
                throw;
            }
        }

        public async Task<PagedResultDto<MovieDetailsDto>> GetAllMoviesAsync(AdminMovieFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting all movies with filter: {@Filter}", filter);

                if (filter == null)
                {
                    throw new ArgumentNullException(nameof(filter));
                }

                // Validate pagination
                if (filter.Page <= 0)
                    filter.Page = 1;

                if (filter.PageSize <= 0 || filter.PageSize > PaginationConstants.MaxPageSize)
                    filter.PageSize = PaginationConstants.DefaultPageSize;

                // ✅ Build query
                var query = _unitOfWork.Movies.GetQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(m =>
                        m.MovieName.ToLower().Contains(searchLower) ||
                        m.MovieDescription.ToLower().Contains(searchLower));
                }

                if (filter.GenreId.HasValue)
                {
                    query = query.Where(m => m.MovieGenres.Any(mg => mg.GenreID == filter.GenreId.Value));
                }

                if (filter.AgeRating.HasValue)
                {
                    query = query.Where(m => m.MovieAgeRating == filter.AgeRating.Value);
                }

                if (filter.ReleaseDateFrom.HasValue)
                {
                    query = query.Where(m => m.ReleaseDate >= filter.ReleaseDateFrom.Value);
                }

                if (filter.ReleaseDateTo.HasValue)
                {
                    query = query.Where(m => m.ReleaseDate <= filter.ReleaseDateTo.Value);
                }

                // Admin can filter by status (Draft, Active, Archived)
                if (filter.Status.HasValue)
                {
                    query = query.Where(m => m.Status == filter.Status.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.Language))
                {
                    query = query.Where(m => m.Language == filter.Language);
                }

                // Get total count before pagination
                var totalCount = await _unitOfWork.Movies.CountAsync(query);

                // ✅ Build orderBy function
                string sortBy = filter.SortBy?.ToLower() ?? "releasedate";
                string sortOrder = filter.SortOrder?.ToLower() ?? "desc";

                Func<IQueryable<Movie>, IOrderedQueryable<Movie>> orderByFunc = sortBy switch
                {
                    "moviename" => sortOrder == "asc"
                        ? q => q.OrderBy(m => m.MovieName)
                        : q => q.OrderByDescending(m => m.MovieName),
                    "rating" => sortOrder == "asc"
                        ? q => q.OrderBy(m => m.MovieRating)
                        : q => q.OrderByDescending(m => m.MovieRating),
                    _ => sortOrder == "asc"
                        ? q => q.OrderBy(m => m.ReleaseDate)
                        : q => q.OrderByDescending(m => m.ReleaseDate)
                };

                var movies = await _unitOfWork.Movies.GetPagedAsync(
                    query: query,
                    orderBy: orderByFunc,
                    skip: (filter.Page - 1) * filter.PageSize,
                    take: filter.PageSize,
                    includeProperties: "CastMembers,MovieImages,MovieGenres.Genre,MovieShowTimes.Hall.Branch"
                );

                var movieDtos = movies.Select(MovieMapper.ToMovieDetailsDto).ToList();

                _logger.LogInformation("Retrieved {Count} movies out of {Total} total",
                    movieDtos.Count, totalCount);

                return new PagedResultDto<MovieDetailsDto>
                {
                    Items = movieDtos,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all movies");
                throw;
            }
        }

        public async Task<MovieDetailsDto> GetMovieAsync(int movieId)
        {
            try
            {
                _logger.LogInformation("Getting movie with movie id {movieId}", movieId);
                if (movieId <= 0)
                    throw new ArgumentException("Movie ID must be a positive integer.", nameof(movieId));
                var movie = await _unitOfWork.Movies.GetMovieWithDetailsByIdAsync(movieId);
                if (movie == null)
                    throw new KeyNotFoundException($"Movie with ID {movieId} not found.");
                _logger.LogInformation("Successfully retrieved movie {MovieId}: {MovieName}", movieId, movie.MovieName);
                return MovieMapper.ToMovieDetailsDto(movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie with movie id {movieId}", movieId);
                throw;
            }
        }
    }
}
