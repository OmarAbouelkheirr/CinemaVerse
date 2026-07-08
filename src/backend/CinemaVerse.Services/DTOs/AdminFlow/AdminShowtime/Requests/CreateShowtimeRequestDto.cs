using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminShowtime.Requests
{
    public class CreateShowtimeRequestDto
    {
        [Required(ErrorMessage = "Movie ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Movie ID must be greater than 0")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Hall ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Hall ID must be greater than 0")]
        public int HallId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Branch ID must be greater than 0")]
        public int? BranchId { get; set; }

        [Required(ErrorMessage = "Show start time is required")]
        public DateTime ShowStartTime { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10000")]
        public decimal Price { get; set; }
    }
}
