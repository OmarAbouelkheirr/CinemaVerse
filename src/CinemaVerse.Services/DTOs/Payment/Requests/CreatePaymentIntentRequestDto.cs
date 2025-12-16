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
        public string? PaymentMethodType { get; set; } = string.Empty;
    }
}
