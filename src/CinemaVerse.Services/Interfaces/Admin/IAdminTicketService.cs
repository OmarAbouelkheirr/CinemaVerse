using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Requests;
using CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Response;
using CinemaVerse.Services.DTOs.Common;

namespace CinemaVerse.Services.Interfaces.Admin
{
    public interface IAdminTicketService
    {
        Task<PagedResultDto<AdminTicketDetailsDto>> GetAllTicketsAsync(AdminTicketFilterDto filter);
        Task<AdminTicketDetailsDto?> GetTicketByIdAsync(int ticketId);
        Task<AdminTicketDetailsDto?> GetTicketByNumberAsync(string ticketNumber);
        Task<AdminTicketDetailsDto?> GetTicketByQrTokenAsync(string qrToken);
        Task<bool> CancelTicketAsync(int ticketId, string cancellationReason);
        Task<List<AdminTicketDetailsDto>> GetTicketsByBookingIdAsync(int bookingId);
        Task<List<AdminTicketDetailsDto>> GetTicketsByShowtimeIdAsync(int showtimeId);

        // لسه لما نتناقش في دي , ممكن نحتاج نعدلها
        Task<bool> MarkTicketAsUsedAsync(int ticketId, string? adminNote = null);
        Task<bool> MarkTicketAsUsedByQrTokenAsync(string qrToken, string? adminNote = null);
    }
}
