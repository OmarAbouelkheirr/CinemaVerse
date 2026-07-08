using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.DTOs.UserFlow.Booking.Helpers
{
    public class ShowtimeDto
    {
        public int MovieShowTimeId { get; set; }
        public DateTime StartTime { get; set; }
        public string MovieTitle { get; set; } = null!;
        public string PosterUrl { get; set; } = null!; 
    }
}
