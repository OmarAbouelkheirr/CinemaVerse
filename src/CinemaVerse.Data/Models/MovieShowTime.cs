using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Data.Models
{
    public class MovieShowTime
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int HallId { get; set; }
        public DateTime ShowStartTime { get; set; }
        //public DateTime ShowEndTime { get; set; } updated to be calculated based on movie duration
        public decimal Price { get; set; }
    }
}
