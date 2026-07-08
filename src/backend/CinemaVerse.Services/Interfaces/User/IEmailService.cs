using CinemaVerse.Services.DTOs.Email.Requests;
using System.Threading.Tasks;

namespace CinemaVerse.Services.Interfaces.User
{
    public interface IEmailService
    {
        Task SendEmailAsync(SendEmailDto emailDto);
        Task SendWelcomeEmailAsync(WelcomeEmailDto emailDto);
        Task SendPasswordResetEmailAsync(PasswordResetEmailDto emailDto);
        Task SendBookingConfirmationEmailAsync(BookingConfirmationEmailDto emailDto);
        Task SendBookingReminderEmailAsync(BookingReminderEmailDto emailDto);
        Task SendPaymentConfirmationEmailAsync(PaymentConfirmationEmailDto emailDto);
        Task SendBookingCancellationEmailAsync(BookingCancellationEmailDto emailDto);
        Task SendEmailVerificationEmailAsync(EmailVerificationEmailDto emailDto);
    }
}