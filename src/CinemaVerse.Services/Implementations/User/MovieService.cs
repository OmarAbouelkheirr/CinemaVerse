using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.Movie.Flow;
using CinemaVerse.Services.DTOs.Movie.Requests;
using CinemaVerse.Services.DTOs.Movie.Response;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.Extensions.Logging;

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

        public async Task<BrowseMoviesResponseDto> BrowseMoviesAsync(BrowseMoviesRequestDto browseDto)
        {
            try
            {
                _logger.LogInformation("Browsing movies with parameters: {@BrowseDto}", browseDto);

                // Validate pagination
                if (browseDto.Page <= 0 || browseDto.PageSize <= 0)
                {
                    _logger.LogWarning("Invalid pagination parameters: Page={Page}, PageSize={PageSize}", browseDto.Page, browseDto.PageSize);
                    throw new ArgumentException("Page and PageSize must be greater than zero.");
                }

                // Validate rating
                if (browseDto.Rating.HasValue && (browseDto.Rating < 0 || browseDto.Rating > 10))
                {
                    _logger.LogWarning("Invalid rating filter: Rating={Rating}", browseDto.Rating);
                    throw new ArgumentOutOfRangeException(nameof(browseDto.Rating), "Rating must be between 0 and 10.");
                }

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
                var query = _unitOfWork.Movies.GetQueryable(); // You need to add this method to IRepository

                // Apply filters before materialization
                if (!string.IsNullOrWhiteSpace(browseDto.Query))
                {
                    query = query.Where(m => m.MovieName.Contains(browseDto.Query));

                }

                if (browseDto.GenreId.HasValue)
                {
                    query = query.Where(m => m.MovieGenres.Any(g => g.GenreID == browseDto.GenreId.Value));
                }

                if (browseDto.Rating.HasValue)
                {
                    query = query.Where(m => m.MovieRating >= (decimal)browseDto.Rating.Value);
                }

                if (browseDto.Date.HasValue)
                {
                    query = query.Where(m => m.ReleaseDate <= browseDto.Date.Value);
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
                var pagedMovies = await _unitOfWork.Movies.GetPagedAsync(
                    query: query,
                    orderBy: q => q.OrderByDescending(m => m.ReleaseDate),
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
                    Status = movie.Status,
                    Cast = movie.MovieCast,
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
                            ShowEndTime = ms.ShowEndTime,
                            HallId = ms.HallId,
                            HallNumber = ms.Hall.HallNumber,
                            BranchId = ms.Hall.BranchId,
                            BranchName = ms.Hall.Branch.BranchName,
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
                MoviePosterImageUrl = movie.MovieImages.FirstOrDefault()?.ImageUrl,
                Genres = movie.MovieGenres.Select(mg => mg.Genre.GenreName).ToList(),
                MovieRating = movie.MovieRating
            };
        }
    }
}
