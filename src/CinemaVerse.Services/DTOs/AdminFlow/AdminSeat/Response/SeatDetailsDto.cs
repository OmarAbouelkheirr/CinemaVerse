namespace CinemaVerse.Services.DTOs.AdminFlow.AdminSeat.Response
{
    public class SeatDetailsDto
    {
        public int SeatId { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public int HallId { get; set; }
        public string HallNumber { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
    }
}
