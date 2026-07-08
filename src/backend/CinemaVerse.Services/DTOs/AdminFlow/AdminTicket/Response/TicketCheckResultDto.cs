using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Response
{
    public class TicketCheckResultDto
    {
        public string TicketNumber { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public string MovieName { get; set; } = string.Empty;
        public DateTime ShowStartTime { get; set; }
        public TimeSpan MovieDuration { get; set; }
        public string HallNumber { get; set; } = string.Empty;
        public HallType HallType { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string BranchName { get; set; } = string.Empty;


        public bool IsFound { get; set; }      
        public string Message { get; set; } = string.Empty;
    }
}
