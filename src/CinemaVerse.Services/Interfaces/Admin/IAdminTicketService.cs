using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Response;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminTicketService
    {
        Task<PagedResultDto<AdminTicketDetailsDto>> GetAllTicketsAsync(AdminTicketFilterDto filter);
        Task<AdminTicketDetailsDto?> GetTicketByIdAsync(int ticketId);
    }
}
