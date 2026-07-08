using System.ComponentModel.DataAnnotations;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Requests
{
    public class UpdateSeatRequestDto
    {
        [StringLength(10, MinimumLength = 1, ErrorMessage = "Seat label must be between 1 and 10 characters")]
        public string? SeatLabel { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Hall ID must be a positive integer")]
        public int? HallId { get; set; }
    }
}
