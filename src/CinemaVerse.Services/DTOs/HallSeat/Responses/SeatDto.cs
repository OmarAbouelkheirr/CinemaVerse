using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.DTOs.HallSeat.Responses
{
    public class SeatDto
    {
        public int SeatId { get; set; }
        public string SeatLabel { get; set; } = string.Empty;
    }
}
