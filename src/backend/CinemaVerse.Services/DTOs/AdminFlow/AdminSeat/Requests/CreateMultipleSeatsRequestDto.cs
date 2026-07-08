using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Requests
{
    public class CreateMultipleSeatsRequestDto
    {
        [Required(ErrorMessage = "Hall ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Hall ID must be a positive integer")]
        public int HallId { get; set; }

        [Required(ErrorMessage = "Seat labels are required")]
        [MinLength(1, ErrorMessage = "At least one seat label is required")]
        public List<string> SeatLabels { get; set; } = new();
    }
}
