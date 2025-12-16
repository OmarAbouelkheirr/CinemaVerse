using CinemaVerse.Services.DTOs.Payment.NewFolder;
using CinemaVerse.Services.DTOs.Payment.Requests;
using CinemaVerse.Services.DTOs.Payment.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<CreatePaymentIntentResponseDto> CreatePaymentIntent(CreatePaymentIntentRequestDto CreatePaymentDto);
        Task <bool> ConfirmPaymentAsync(ConfirmPaymentRequestDto ConfrimPaymentDto);
        Task<bool> RefundPaymentAsync(RefundPaymentRequestDto RefundPaymentDto);
    }
}
