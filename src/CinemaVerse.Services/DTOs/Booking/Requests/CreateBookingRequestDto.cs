using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.DTOs.Booking.Requests
{
    public class CreateBookingRequestDto
    {
        public int MovieShowTimeId { get; set; }
        public List<int> SeatIds { get; set; } = new();
    }
}
