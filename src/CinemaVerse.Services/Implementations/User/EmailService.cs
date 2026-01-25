using CinemaVerse.Services.DTOs.Email.Requests;
using CinemaVerse.Services.Interfaces.User;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using RazorLight;
using System.Reflection;

namespace CinemaVerse.Services.Implementations.User
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly RazorLightEngine _razorEngine;
        private readonly string _assemblyName;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "CinemaVerse.Services";
            
            _razorEngine = new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(typeof(EmailService).Assembly)
                .UseMemoryCachingProvider()
                .Build();
        }
        public async Task SendBookingCancellationEmailAsync(BookingCancellationEmailDto emailDto)
        {
            if (emailDto == null)
            {
                _logger.LogWarning("BookingCancellationEmailDto is null");
                throw new ArgumentNullException(nameof(emailDto));
            }

            try
            {
                _logger.LogInformation("Sending booking cancellation email to {Email} for BookingId: {BookingId}",
                    emailDto.To, emailDto.BookingId);

                // ✅ Render Razor template
                string htmlBody = await _razorEngine.CompileRenderAsync($"{_assemblyName}.Helpers.Templates.BookingCancellationEmail.cshtml", emailDto);

                await SendEmailAsync(new SendEmailDto
                {
                    To = emailDto.To,
                    Subject = $"Booking Cancelled - {emailDto.MovieName}",
                    Body = htmlBody
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending cancellation email to {Email}", emailDto.To);
                throw;
            }
        }        

        public async Task SendBookingConfirmationEmailAsync(BookingConfirmationEmailDto emailDto)
        {
            if (emailDto == null)
            {
                _logger.LogWarning("BookingConfirmationEmailDto is null");
                throw new ArgumentNullException(nameof(emailDto));
            }

            try
            {
                _logger.LogInformation("Sending booking confirmation email to {Email} for BookingId: {BookingId}",
                    emailDto.To, emailDto.BookingId);

                // ✅ Render Razor template
                string htmlBody = await _razorEngine.CompileRenderAsync($"{_assemblyName}.Helpers.Templates.BookingConfirmationEmail.cshtml", emailDto);

                await SendEmailAsync(new SendEmailDto
                {
                    To = emailDto.To,
                    Subject = $"Booking Confirmation - {emailDto.MovieName} 🎟️",
                    Body = htmlBody
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending booking confirmation email to {Email}", emailDto.To);
                throw;
            }
        }

        public async Task SendEmailAsync(SendEmailDto emailDto)
        {
            try
            {
                if (emailDto == null)
                {
                    _logger.LogWarning("SendEmailDto is null");
                    throw new ArgumentNullException(nameof(emailDto));
                }

                _logger.LogInformation("Sending email to {Email} with subject {Subject}", emailDto.To, emailDto.Subject);
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _configuration["EmailSettings:SenderName"] ?? "CinemaVerse",
                    _configuration["EmailSettings:SenderEmail"] 
                        ?? throw new InvalidOperationException("SenderEmail not configured in appsettings.json")));
                message.To.Add(MailboxAddress.Parse(emailDto.To));
                message.Subject = emailDto.Subject;
                var bodyBuilder = new BodyBuilder();

                bodyBuilder.HtmlBody = emailDto.Body;

                message.Body = bodyBuilder.ToMessageBody();

                var host = _configuration["EmailSettings:SmtpServer"] 
                    ?? throw new InvalidOperationException("SmtpServer not configured in appsettings.json");
                var port = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var username = _configuration["EmailSettings:Username"] 
                    ?? throw new InvalidOperationException("Username not configured in appsettings.json");
                var password = _configuration["EmailSettings:Password"] 
                    ?? throw new InvalidOperationException("Password not configured in appsettings.json");

                var useSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
                var secureOption = useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(host, port, secureOption);
                await smtpClient.AuthenticateAsync(username, password);

                // Send message
                await smtpClient.SendAsync(message);

                // Disconnect
                await smtpClient.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", emailDto.To);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", emailDto.To);
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(PasswordResetEmailDto emailDto)
        {
            if (emailDto == null)
            {
                _logger.LogWarning("PasswordResetEmailDto is null");
                throw new ArgumentNullException(nameof(emailDto));
            }

            try
            {
                _logger.LogInformation("Sending password reset email to {Email}", emailDto.To);

                string htmlBody = await _razorEngine.CompileRenderAsync($"{_assemblyName}.Helpers.Templates.PasswordResetEmail.cshtml", emailDto);

                await SendEmailAsync(new SendEmailDto
                {
                    To = emailDto.To,
                    Subject = "Reset Your Password - CinemaVerse",
                    Body = htmlBody,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", emailDto.To);
                throw;
            }
        }

        public async Task SendPaymentConfirmationEmailAsync(PaymentConfirmationEmailDto emailDto)
        {
            try
            {
                _logger.LogInformation("Sending payment confirmation email to {Email} for BookingId: {BookingId}",
                    emailDto.To, emailDto.BookingId);

                // ✅ Render Razor template
                string htmlBody = await _razorEngine.CompileRenderAsync($"{_assemblyName}.Helpers.Templates.PaymentConfirmationEmail.cshtml", emailDto);

                await SendEmailAsync(new SendEmailDto
                {
                    To = emailDto.To,
                    Subject = $"Payment Successful - Booking #{emailDto.BookingId} ✅",
                    Body = htmlBody
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment confirmation email to {Email}", emailDto.To);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(WelcomeEmailDto emailDto)
        {
            if (emailDto == null)
            {
                _logger.LogWarning("WelcomeEmailDto is null");
                throw new ArgumentNullException(nameof(emailDto));
            }

            try
            {
                _logger.LogInformation("Sending Welcome email to {Email} with subject {Subject}", emailDto.To, emailDto.Subject);
                string htmlBody = await _razorEngine.CompileRenderAsync($"{_assemblyName}.Helpers.Templates.WelcomeEmail.cshtml", emailDto);
                await SendEmailAsync(new SendEmailDto
                {
                    To = emailDto.To,
                    Subject = emailDto.Subject,
                    Body = htmlBody
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Welcome mail to {Email}", emailDto.To);
                throw;
            }
        }
    }
}
