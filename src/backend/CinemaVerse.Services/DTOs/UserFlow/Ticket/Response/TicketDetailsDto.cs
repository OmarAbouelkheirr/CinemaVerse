using CinemaVerse.Data.Enums;
using System;

namespace CinemaVerse.Services.DTOs.Ticket.Response
{
    public class TicketDetailsDto
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string MovieName { get; set; } = string.Empty;
        public DateTime ShowStartTime { get; set; }
        public TimeSpan MovieDuration { get; set; }
        public string HallNumber { get; set; } = string.Empty;
        public HallType HallType { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
        public string MoviePoster { get; set; } = string.Empty;
        public MovieAgeRating MovieAgeRating { get; set; }
        public string QrToken { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public decimal Price { get; set; }
        public string BranchName { get; set; } = string.Empty;

    }
}
