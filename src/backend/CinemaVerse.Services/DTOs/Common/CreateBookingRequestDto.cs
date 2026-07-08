using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.Common
{
    public class CreateBookingRequestDto
    {
        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be greater than 0")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Movie showtime ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Movie showtime ID must be greater than 0")]
        public int MovieShowTimeId { get; set; }

        [Required(ErrorMessage = "At least one seat is required")]
        [MinLength(1, ErrorMessage = "At least one seat must be selected")]
        public List<int> SeatIds { get; set; } = new();
    }
}
