using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Services.Interfaces
{
    public interface IEmailService

    {
        Task SendEmailAsync(string To, string Subject, string Body);
        Task SendWelcomeEmailAsync(string To,string Username, string Subject, string Body);
        Task SendPasswordResetEmailAsync(string To, string Subject, string Body);
        Task SendBookingConfirmationEmailAsync(string To, string Subject, string Body);
        Task SendPaymentConfiramionEmailAsync(string To , string Subject , string Body);
        Task SendBookingCancellationEmailAsync(string To, string Subject, string Body);


    }
}
