namespace CinemaVerse.Services.DTOs.HallSeat.Responses
{
    public class HallWithSeatsDto
    {
        public int HallId { get; set; }
        public string HallNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }

        public List<SeatDto> Seats { get; set; } = new();
    }
}
