using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.DTOs.Payment.Requests
{
    public class ConfirmPaymentRequestDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public int BookingId { get; set; }
    }
}
