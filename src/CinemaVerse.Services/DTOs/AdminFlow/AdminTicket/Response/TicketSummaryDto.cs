namespace CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Response
{
    public class TicketSummaryDto
    {
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int UsedTickets { get; set; }
        public int CancelledTickets { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
