using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.UserFlow.Review.Requests
{
    public class UpdateReviewRequestDto
    {
        [Required(ErrorMessage = "Rating is required")]
        [Range(0.01, 9.99, ErrorMessage = "Rating must be between 0.01 and 9.99 (DB precision 3,2)")]
        public decimal Rating { get; set; }

        [StringLength(2000, ErrorMessage = "Comment must not exceed 2000 characters")]
        public string? Comment { get; set; }
    }
}
