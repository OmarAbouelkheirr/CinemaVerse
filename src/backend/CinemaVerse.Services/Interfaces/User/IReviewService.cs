using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.UserFlow.Review.Requests;
using CinemaVerse.Services.DTOs.UserFlow.Review.Responses;

namespace CinemaVerse.Services.Interfaces.User
{
    public interface IReviewService
    {
        Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewRequestDto dto);
        Task<ReviewDto> UpdateReviewAsync(int userId, int reviewId, UpdateReviewRequestDto dto);
        Task<bool> DeleteReviewAsync(int userId, int reviewId);
        Task<PagedResultDto<ReviewDto>> GetReviewsByMovieIdAsync(int movieId, ReviewListFilterDto filter);
        Task<PagedResultDto<ReviewDto>> GetUserReviewsAsync(int userId, ReviewListFilterDto filter);
        Task<ReviewDto?> GetUserReviewForMovieAsync(int userId, int movieId);
    }
}
