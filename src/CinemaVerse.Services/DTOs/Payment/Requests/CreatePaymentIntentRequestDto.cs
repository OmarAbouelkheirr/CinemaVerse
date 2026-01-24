using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.DTOs.Payment.NewFolder
{
    public class CreatePaymentIntentRequestDto
    {
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public string PaymentMethodType { get; set; } = string.Empty;
    }
}
