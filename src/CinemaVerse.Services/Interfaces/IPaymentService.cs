using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.Interfaces
{
    public interface IPaymentService
    {
        Task CreatePaymentIntent();
        Task <bool> ConfirmPaymentAsync();
        Task RefundPaymentAsync();
    }
}
