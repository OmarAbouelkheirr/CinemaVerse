using CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Response;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminHall.Response
{
    public class HallDetailsResponseDto
    {
        public int BranchId { get; set; }
        public int Capacity { get; set; }
        public string HallNumber { get; set; } = string.Empty;
        public string HallStatus { get; set; } = string.Empty;
        public string HallType { get; set; } = string.Empty;
        public List<SeatDetailsDto> Seats { get; set; } = new();
    }
}
