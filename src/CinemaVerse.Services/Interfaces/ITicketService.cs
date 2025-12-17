using CinemaVerse.Services.DTOs.Ticket.Response;


namespace CinemaVerse.Services.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketDetailsDto>> IssueTicketsAsync(int bookingId);

    }
}
