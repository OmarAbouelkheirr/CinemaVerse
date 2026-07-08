using CinemaVerse.Data.Enums;
using System;

namespace CinemaVerse.Services.DTOs.Ticket.Response
{
    public class TicketListItemDto
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public DateTime ShowStartTime { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public string MoviePoster { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public decimal Price { get; set; }
    }
}
