using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Response;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminTicketService
    {
        Task<PagedResultDto<AdminTicketListItemDto>> GetAllTicketsAsync(AdminTicketFilterDto filter);
        Task<AdminTicketDetailsDto> GetTicketByIdAsync(int ticketId);
        Task<TicketCheckResultDto> CheckTicketByQrTokenAsync(string qrToken);
        //Task<bool> CancelTicketAsync(int ticketId); // مش دلوقتي
        Task<List<AdminTicketDetailsDto>> GetTicketsByBookingIdAsync(int bookingId);
        Task<List<AdminTicketDetailsDto>> GetTicketsByShowtimeIdAsync(int showtimeId);
        Task<TicketCheckInResultDto> MarkTicketAsUsedAsync(string qrToken);
    }
}
