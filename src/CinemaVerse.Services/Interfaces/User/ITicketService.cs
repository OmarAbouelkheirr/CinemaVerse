using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.Ticket.Response;


namespace CinemaVerse.Services.Interfaces.User
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketDetailsDto>> IssueTicketsAsync(int bookingId);
        TicketDetailsDto MapToDto(Ticket ticket);
    }
}
