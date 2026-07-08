namespace CinemaVerse.Services.DTOs.Email.Flow
{
    public class TicketInfoDto
    {
        public string TicketNumber { get; set; } = string.Empty;
        public string SeatNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
