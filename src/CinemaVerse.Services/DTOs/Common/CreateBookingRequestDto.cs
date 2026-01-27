using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.Common
{
    public class CreateBookingRequestDto
    {

        public int UserId { get; set; }
        [Required]
        public int MovieShowTimeId { get; set; }
        [Required]
        public List<int> SeatIds { get; set; } = new();
    }
}
