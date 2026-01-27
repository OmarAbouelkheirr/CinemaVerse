namespace CinemaVerse.Services.DTOs.AdminFlow.AdminShowtime.Response
{
    public class ShowtimeDetailsDto
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public int HallId { get; set; }
        public string HallNumber { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public DateTime ShowStartTime { get; set; }
        public DateTime ShowEndTime { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalBookings { get; set; }
        public int TotalTickets { get; set; }
    }
}
