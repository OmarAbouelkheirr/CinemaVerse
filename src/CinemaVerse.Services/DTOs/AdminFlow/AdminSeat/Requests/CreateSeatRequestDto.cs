using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Requests
{
    public class CreateSeatRequestDto
    {
        [Required(ErrorMessage = "Seat label is required")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "Seat label must be between 1 and 10 characters")]
        public string SeatLabel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hall ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Hall ID must be a positive integer")]
        public int HallId { get; set; }
    }
}
