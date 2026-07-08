namespace CinemaVerse.Services.DTOs.UserFlow.Review.Responses
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int MovieId { get; set; }
        public string? MovieName { get; set; }
        public decimal Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
