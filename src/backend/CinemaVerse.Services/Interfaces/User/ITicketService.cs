using CinemaVerse.Data.Models;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests;
using CinemaVerse.Services.DTOs.Common;
using CinemaVerse.Services.DTOs.Ticket.Response;


namespace CinemaVerse.Services.Interfaces.User
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketDetailsDto>> IssueTicketsAsync(int bookingId);
        TicketDetailsDto MapToDto(Ticket ticket);
        Task<PagedResultDto<TicketDetailsDto>> GetUserTicketsAsync(int userId, AdminTicketFilterDto filter);
        Task<TicketDetailsDto> GetUserTicketByIdAsync(int userId, int ticketId);
    }
}
