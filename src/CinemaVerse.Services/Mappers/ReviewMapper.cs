using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.UserFlow.Review.Responses;

namespace CinemaVerse.Services.Mappers
{
    public static class ReviewMapper
    {
        public static ReviewDto ToReviewDto(Review review, string userName, string? movieName = null)
        {
            return new ReviewDto
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = userName,
                MovieId = review.MovieId,
                MovieName = movieName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt
            };
        }
    }
}
