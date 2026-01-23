using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;

namespace CinemaVerse.Services.DTOs.HallSeat.Responses
{
    public class HallWithSeatsDto
    {
        public int HallId { get; set; }
        public string HallNumber { get; set; } = string.Empty;
        public HallType HallType { get; set; }
        public string Branch { get; set; } = null!;
        public int Capacity { get; set; }

        public List<SeatDto> AvailableSeats { get; set; } = new();
        public List<SeatDto> ReservedSeats { get; set; } = new();
    }
}
