namespace CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Response
{
    public enum TicketCheckInResult
    {
        Success,
        NotFound,
        AlreadyUsed,
        Cancelled,
        InvalidStatus,
        Error
    }

    public class TicketCheckInResultDto
    {
        public bool Success { get; set; }
        public TicketCheckInResult Result { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
