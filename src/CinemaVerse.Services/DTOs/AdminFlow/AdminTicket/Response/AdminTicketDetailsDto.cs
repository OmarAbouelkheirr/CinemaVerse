using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaVerse.Data.Enums;
using CinemaVerse.Services.DTOs.Ticket.Response;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Response
{
    public class AdminTicketDetailsDto : TicketDetailsDto
    {
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public DateTime? UsedAt { get; set; }
        public string? AdminNote { get; set; }
    }
}
