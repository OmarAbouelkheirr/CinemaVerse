using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.UserFlow.Movie.Flow;
using CinemaVerse.Services.DTOs.Movie.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Movie.Response;
using CinemaVerse.Services.Interfaces.User;
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

                if (browseDto == null)
                {
                    _logger.LogWarning("BrowseMoviesRequestDto is null");
                    throw new ArgumentNullException(nameof(browseDto));
                }

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
                    {
                        _logger.LogWarning("Genre with ID {GenreId} does not exist.", browseDto.GenreId.Value);
                        throw new ArgumentException($"Genre with ID {browseDto.GenreId.Value} does not exist.");
                    }
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

                var movieCards = pagedMovies.Select(MapToMovieCardDto).ToList();

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

        public async Task<MovieDetailsDto?> GetMovieDetailsAsync(int movieId)
        {
            try
            {
                _logger.LogInformation("Getting movie details for id: {movieId}", movieId);

                if (movieId <= 0)
                {
                    _logger.LogWarning("Invalid movie ID: {movieId}", movieId);
                    throw new ArgumentException("Movie ID must be greater than zero.", nameof(movieId));
                }

                // Fetch movie with related data (now includes all nested navigation)
                var movie = await _unitOfWork.Movies.GetMovieWithDetailsByIdAsync(movieId);

                if (movie == null)
                {
                    _logger.LogWarning("Movie with ID {movieId} not found.", movieId);
                    return null;
                }

                var movieDetails = new MovieDetailsDto
                {
                    MovieId = movie.Id,
                    MovieName = movie.MovieName,
                    MovieDescription = movie.MovieDescription,
                    MovieDuration = movie.MovieDuration,
                    ReleaseDate = movie.ReleaseDate,
                    MovieAgeRating = movie.MovieAgeRating,
                    MovieRating = movie.MovieRating,
                    TrailerUrl = movie.TrailerUrl,
                    MoviePoster = movie.MoviePoster ?? string.Empty,
                    Status = movie.Status,
                    CastMembers = movie.CastMembers?
                        .OrderBy(c => c.DisplayOrder)
                        .Select(c => new CastMemberDto
                        {
                            Id = c.Id,
                            PersonName = c.PersonName,
                            ImageUrl = c.ImageUrl,
                            RoleType = c.RoleType,
                            CharacterName = c.CharacterName,
                            DisplayOrder = c.DisplayOrder,
                            IsLead = c.IsLead
                        }).ToList() ?? new List<CastMemberDto>(),
                    Genres = movie.MovieGenres
                        .Where(mg => mg.Genre != null) // Safety check
                        .Select(mg => new GenreDto
                        {
                            GenreId = mg.Genre.Id,
                            Name = mg.Genre.GenreName
                        }).ToList(),
                    Images = movie.MovieImages.Select(mi => new MovieImageDto
                    {
                        Id = mi.Id,
                        ImageUrl = mi.ImageUrl
                    }).ToList(),
                    ShowTimes = movie.MovieShowTimes
                        .Where(ms => ms.Hall != null && ms.Hall.Branch != null) // Safety check
                        .Select(ms => new MovieShowTimeDto
                        {
                            MovieShowTimeId = ms.Id,
                            ShowStartTime = ms.ShowStartTime,
                            HallId = ms.HallId,
                            HallNumber = ms.Hall.HallNumber,
                            HallType = ms.Hall.HallType,
                            BranchId = ms.Hall.BranchId,
                            BranchName = ms.Hall.Branch.BranchName,
                            BranchLocation = ms.Hall.Branch.BranchLocation,
                            TicketPrice = ms.Price
                        }).ToList()
                };

                _logger.LogInformation("Successfully retrieved details for movie ID {movieId}", movieId);
                return movieDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movie details for ID {movieId}", movieId);
                throw;
            }
        }





        private MovieCardDto MapToMovieCardDto(Movie movie)
        {
            return new MovieCardDto
            {
                MovieId = movie.Id,
                MovieName = movie.MovieName,
                MoviePosterImageUrl = movie.MoviePoster ?? string.Empty,
                MovieDuration = movie.MovieDuration.ToString(@"hh\:mm"),
                Genres = movie.MovieGenres.Select(mg => mg.Genre.GenreName).ToList(),
                MovieRating = movie.MovieRating
            };
        }
    }
}
