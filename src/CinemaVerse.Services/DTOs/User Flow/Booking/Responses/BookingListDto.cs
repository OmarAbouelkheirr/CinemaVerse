using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaVerse.Data.Enums;

namespace CinemaVerse.Services.DTOs.Booking.Responses
{
    public class BookingListDto
    {
        public int BookingId { get; set; }
        public BookingStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        public int MovieShowTimeId { get; set; }
        public DateTime ShowStartTime { get; set; }
        public string MovieTitle { get; set; } = null!;

        public int TicketsCount { get; set; }
    }
}
