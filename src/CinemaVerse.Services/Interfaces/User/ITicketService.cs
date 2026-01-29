using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.Ticket.Response;


namespace CinemaVerse.Services.Interfaces.User
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketDetailsDto>> IssueTicketsAsync(int bookingId);
        TicketDetailsDto MapToDto(Ticket ticket);
        Task<List<TicketListItemDto>> GetUserTicketsAsync(int userId);
        Task<TicketDetailsDto?> GetUserTicketByIdAsync(int userId, int ticketId);
    }
}
