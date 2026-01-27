using System;
using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.AdminFlow.AdminTicket.Response
{
    /// <summary>
    /// Lightweight ticket representation for admin list views.
    /// </summary>
    public class AdminTicketListItemDto
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public DateTime ShowStartTime { get; set; }
        public TicketStatus Status { get; set; }
        public decimal Price { get; set; }
        public string BranchName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public BookingStatus BookingStatus { get; set; }
    }
}

