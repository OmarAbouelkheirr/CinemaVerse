using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Constants;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.UserFlow.Review.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Review.Responses;
using CinemaVerse.Services.Interfaces.User;
using CinemaVerse.Services.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.User
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(IUnitOfWork unitOfWork, ILogger<ReviewService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewRequestDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var movie = await _unitOfWork.Movies.GetByIdAsync(dto.MovieId);
            if (movie == null)
                throw new KeyNotFoundException($"Movie with ID {dto.MovieId} not found.");

            if (movie.Status != MovieStatus.Active)
                throw new InvalidOperationException("Reviews can only be added for active movies.");

            var existingReview = await _unitOfWork.Reviews.FirstOrDefaultAsync(r =>
                r.UserId == userId && r.MovieId == dto.MovieId);

            if (existingReview != null)
                throw new InvalidOperationException("You have already submitted a review for this movie. You can update it from your review.");

            var review = new Review
            {
                UserId = userId,
                MovieId = dto.MovieId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Reviews.AddAsync(review);
            _logger.LogInformation("Created review for user {UserId}, movie {MovieId}", userId, dto.MovieId);

            await _unitOfWork.SaveChangesAsync();
            await UpdateMovieRatingFromReviewsAsync(dto.MovieId);

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return ReviewMapper.ToReviewDto(review, user?.FullName ?? string.Empty);
        }

        public async Task<ReviewDto> UpdateReviewAsync(int userId, int reviewId, UpdateReviewRequestDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {reviewId} not found.");

            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You can only edit your own review.");

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Reviews.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();
            await UpdateMovieRatingFromReviewsAsync(review.MovieId);

            _logger.LogInformation("Updated review {ReviewId} for user {UserId}", reviewId, userId);

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return ReviewMapper.ToReviewDto(review, user?.FullName ?? string.Empty);
        }

        public async Task<bool> DeleteReviewAsync(int userId, int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {reviewId} not found.");

            if (review.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own review.");

            var movieId = review.MovieId;
            await _unitOfWork.Reviews.DeleteAsync(review);
            await _unitOfWork.SaveChangesAsync();
            await UpdateMovieRatingFromReviewsAsync(movieId);

            _logger.LogInformation("Deleted review {ReviewId} for user {UserId}", reviewId, userId);
            return true;
        }

        public async Task<PagedResultDto<ReviewDto>> GetReviewsByMovieIdAsync(int movieId, ReviewListFilterDto filter)
        {
            if (filter.Page <= 0)
                filter.Page = 1;
            if (filter.PageSize <= 0 || filter.PageSize > PaginationConstants.MaxPageSize)
                filter.PageSize = PaginationConstants.DefaultPageSize;

            var query = _unitOfWork.Reviews.GetQueryable()
                .Where(r => r.MovieId == movieId);

            var totalCount = await _unitOfWork.Reviews.CountAsync(query);

            var reviews = await _unitOfWork.Reviews.GetPagedAsync(
                query,
                q => q.OrderByDescending(r => r.CreatedAt),
                skip: (filter.Page - 1) * filter.PageSize,
                take: filter.PageSize,
                includeProperties: "User");

            var items = reviews.Select(r => ReviewMapper.ToReviewDto(r, r.User?.FullName ?? string.Empty)).ToList();

            return new PagedResultDto<ReviewDto>
            {
                Items = items,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PagedResultDto<ReviewDto>> GetUserReviewsAsync(int userId, ReviewListFilterDto filter)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be a positive integer.", nameof(userId));

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            if (filter.Page <= 0)
                filter.Page = 1;
            if (filter.PageSize <= 0 || filter.PageSize > PaginationConstants.MaxPageSize)
                filter.PageSize = PaginationConstants.DefaultPageSize;

            var query = _unitOfWork.Reviews.GetQueryable()
                .Where(r => r.UserId == userId);

            var totalCount = await _unitOfWork.Reviews.CountAsync(query);

            var reviews = await _unitOfWork.Reviews.GetPagedAsync(
                query,
                q => q.OrderByDescending(r => r.CreatedAt),
                skip: (filter.Page - 1) * filter.PageSize,
                take: filter.PageSize,
                includeProperties: "User,Movie");

            var items = reviews.Select(r => ReviewMapper.ToReviewDto(r, r.User?.FullName ?? string.Empty, r.Movie?.MovieName)).ToList();

            return new PagedResultDto<ReviewDto>
            {
                Items = items,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ReviewDto?> GetUserReviewForMovieAsync(int userId, int movieId)
        {
            var review = await _unitOfWork.Reviews.FirstOrDefaultAsync(r =>
                r.UserId == userId && r.MovieId == movieId);

            if (review == null)
                return null;

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return ReviewMapper.ToReviewDto(review, user?.FullName ?? string.Empty);
        }

        private async Task UpdateMovieRatingFromReviewsAsync(int movieId)
        {
            var movie = await _unitOfWork.Movies.GetByIdAsync(movieId);
            if (movie == null)
                return;

            var avgRating = await _unitOfWork.Reviews
                .GetQueryable()
                .Where(r => r.MovieId == movieId)
                .Select(r => r.Rating)
                .DefaultIfEmpty()
                .AverageAsync();

            movie.MovieRating = avgRating;
            await _unitOfWork.Movies.UpdateAsync(movie);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Updated movie {MovieId} rating to {Rating}", movieId, movie.MovieRating);
        }

            }
}
