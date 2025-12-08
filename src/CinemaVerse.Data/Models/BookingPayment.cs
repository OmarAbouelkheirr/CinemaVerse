using CinemaVerse.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Data.Models
{
    public class BookingPayment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentInentId { get; set; } = string.Empty;
        public DateTime TrasnactionDate { get; set; } 
        public string Currency { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
    }
}
