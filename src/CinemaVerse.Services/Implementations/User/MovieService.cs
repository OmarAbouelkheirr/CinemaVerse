using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.UserFlow.Movie.Flow;
using CinemaVerse.Services.DTOs.Movie.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Movie.Response;
using CinemaVerse.Services.Interfaces.User;
using CinemaVerse.Services.Mappers;
using Microsoft.Extensions.Logging;
using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.Implementations.User
{
    public class MovieService : IMovieService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MovieService> _logger;

        public MovieService(IUnitOfWork unitOfWork, ILogger<MovieService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async  Task<BrowseMoviesResponseDto> BrowseMoviesAsync(BrowseMoviesFilterDto browseDto)
        {
            try
            {
                _logger.LogInformation("Browsing movies with parameters: {@BrowseDto}", browseDto);

                // Validate pagination (consistent with Admin services)
                if (browseDto.Page <= 0)
                    browseDto.Page = 1;

                if (browseDto.PageSize <= 0 || browseDto.PageSize > PaginationConstants.MaxPageSize)
                    browseDto.PageSize = PaginationConstants.DefaultPageSize;

                // Validate genre existence (optional optimization: remove if genre filter is common)
                if (browseDto.GenreId.HasValue)
                {
                    var genreExists = await _unitOfWork.Genres.AnyAsync(g => g.Id == browseDto.GenreId.Value);
                    if (!genreExists)
                        throw new ArgumentException($"Genre with ID {browseDto.GenreId.Value} does not exist.");
                }

                // Build query at database level (do NOT materialize yet)
                var query = _unitOfWork.Movies.GetQueryable();

                // Apply filters before materialization
                if (!string.IsNullOrWhiteSpace(browseDto.SearchTerm))
                {
                    var searchLower = browseDto.SearchTerm.ToLower();
                    query = query.Where(m =>
                        m.MovieName.ToLower().Contains(searchLower) ||
                        m.MovieDescription.ToLower().Contains(searchLower));
                }

                if (browseDto.GenreId.HasValue)
                {
                    query = query.Where(m => m.MovieGenres.Any(g => g.GenreID == browseDto.GenreId.Value));
                }

                if (browseDto.AgeRating.HasValue)
                {
                    query = query.Where(m => m.MovieAgeRating == browseDto.AgeRating.Value);
                }

                if (browseDto.ReleaseDateFrom.HasValue)
                {
                    query = query.Where(m => m.ReleaseDate >= browseDto.ReleaseDateFrom.Value);
                }

                if (browseDto.ReleaseDateTo.HasValue)
                {
                    query = query.Where(m => m.ReleaseDate <= browseDto.ReleaseDateTo.Value);
                }

                // Status filter (optional) - if not provided, return all statuses
                if (browseDto.Status.HasValue)
                {
                    query = query.Where(m => m.Status == browseDto.Status.Value);
                }

                if (!string.IsNullOrWhiteSpace(browseDto.Language))
                {
                    query = query.Where(m => m.Language == browseDto.Language);
                }

                // Get total count before pagination
                var totalCount = await _unitOfWork.Movies.CountAsync(query);

                if (totalCount == 0)
                {
                    _logger.LogInformation("No movies found after applying filters.");
                    return new BrowseMoviesResponseDto
                    {
                        Movies = new List<MovieCardDto>(),
                        Page = browseDto.Page,
                        PageSize = browseDto.PageSize,
                        TotalCount = 0
                    };
                }

                // Apply ordering and pagination at database level
                string sortBy = browseDto.SortBy?.ToLower() ?? "releasedate";
                string sortOrder = browseDto.SortOrder?.ToLower() ?? "desc";

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
                        : q => q.OrderByDescending(m => m.ReleaseDate),
                };

                var pagedMovies = await _unitOfWork.Movies.GetPagedAsync(
                    query: query,
                    orderBy: orderByFunc,
                    skip: (browseDto.Page - 1) * browseDto.PageSize,
                    take: browseDto.PageSize,
                    includeProperties: "MovieImages,MovieGenres.Genre" // Eager load navigation properties
                );

                var movieCards = pagedMovies.Select(MovieMapper.ToMovieCardDto).ToList();

                _logger.LogInformation("Returning {Count} movies for page {Page} with page size {PageSize}", movieCards.Count, browseDto.Page, browseDto.PageSize);

                return new BrowseMoviesResponseDto
                {
                    Movies = movieCards,
                    Page = browseDto.Page,
                    PageSize = browseDto.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while browsing movies.");
                throw;
            }
        }

        public async Task<MovieDetailsDto> GetMovieDetailsAsync(int movieId)
        {
            try
            {
                _logger.LogInformation("Getting movie details for id: {movieId}", movieId);

                if (movieId <= 0)
                    throw new ArgumentException("Movie ID must be a positive integer.", nameof(movieId));
                var movie = await _unitOfWork.Movies.GetMovieWithDetailsByIdAsync(movieId);
                if (movie == null)
                    throw new KeyNotFoundException($"Movie with ID {movieId} not found.");

                _logger.LogInformation("Successfully retrieved details for movie ID {movieId}", movieId);
                return MovieMapper.ToMovieDetailsDto(movie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie details for ID {movieId}", movieId);
                throw;
            }
        }





            }
}
