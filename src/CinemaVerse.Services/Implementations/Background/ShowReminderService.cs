using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.DTOs.Email.Flow;
using CinemaVerse.Services.DTOs.Email.Requests;
using CinemaVerse.Services.Interfaces.Background;
using CinemaVerse.Services.Interfaces.User;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.Background
{
    public class ShowReminderService : IShowReminderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<ShowReminderService> _logger;

        private const int WindowStartMinutes = 110;
        private const int WindowEndMinutes = 130;

        public ShowReminderService(IUnitOfWork unitOfWork,IEmailService emailService,ILogger<ShowReminderService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<int> SendShowRemindersAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(WindowStartMinutes);
            var windowEnd = now.AddMinutes(WindowEndMinutes);

            var bookings = await _unitOfWork.Bookings.GetConfirmedBookingsInShowTimeWindowAsync(windowStart, windowEnd);
            var sentCount = 0;

            foreach (var booking in bookings)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (booking.User?.IsEmailConfirmed != true)
                    continue;

                if (string.IsNullOrWhiteSpace(booking.User?.Email))
                    continue;

                try
                {
                    var dto = new BookingReminderEmailDto
                    {
                        To = booking.User!.Email,
                        BookingId = booking.Id,
                        FullName = booking.User.FullName,
                        MovieName = booking.MovieShowTime?.Movie?.MovieName ?? string.Empty,
                        ShowStartTime = booking.MovieShowTime?.ShowStartTime ?? DateTime.MinValue,
                        ShowEndTime = booking.MovieShowTime?.ShowEndTime ?? DateTime.MinValue,
                        HallNumber = booking.MovieShowTime?.Hall?.HallNumber ?? string.Empty,
                        BranchName = booking.MovieShowTime?.Hall?.Branch?.BranchName ?? string.Empty,
                        TotalAmount = booking.TotalAmount,
                        Currency = "EGP",
                        Tickets = booking.Tickets?.Select(t => new TicketInfoDto
                        {
                            TicketNumber = t.TicketNumber,
                            SeatNumber = t.Seat?.SeatLabel ?? string.Empty,
                            Price = t.Price
                        }).ToList() ?? new List<TicketInfoDto>()
                    };

                    await _emailService.SendBookingReminderEmailAsync(dto);
                    sentCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ShowReminder: failed to send reminder email for BookingId {BookingId}, user {Email}",
                        booking.Id, booking.User?.Email);
                }
            }

            if (sentCount > 0)
                _logger.LogInformation("ShowReminder: sent {Count} reminder email(s).", sentCount);

            return sentCount;
        }
    }
}
