using CinemaVerse.Services.DTOs.Ticket.Response;


namespace CinemaVerse.Services.Interfaces
{
    public interface ITicketService
    {
        Task<TicketDetailsDto> IssueTicketAsync(int bookingId);
        Task<string> GenerateTicketQrAsync(int TicketId);

    }
}
