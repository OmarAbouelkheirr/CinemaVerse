using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.DTOs.Payment.Requests
{
    public class RefundPaymentRequestDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public decimal RefundAmount { get; set; }   
    }
}
