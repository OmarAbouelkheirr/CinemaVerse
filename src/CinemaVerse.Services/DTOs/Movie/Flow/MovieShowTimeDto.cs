using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.DTOs.Movie.Flow
{
    public class MovieShowTimeDto
    {
        public int MovieShowTimeId { get; set; }
        public DateTime ShowStartTime { get; set; }
        public DateTime MovieEndTime { get; set; } 
        public int HallId { get; set; } 
        public string HallNumber { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public decimal TicketPrice { get; set; }
    }
}
