using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Repositories;
using CinemaVerse.Services.Interfaces.Background;
using Microsoft.Extensions.Logging;

namespace CinemaVerse.Services.Implementations.Background
{
    public class ExpirePendingBookingsService : IExpirePendingBookingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExpirePendingBookingsService> _logger;

        public ExpirePendingBookingsService(IUnitOfWork unitOfWork, ILogger<ExpirePendingBookingsService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> ExpirePendingBookingsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var expiredPending = await _unitOfWork.Bookings.FindAllAsync(b =>
                b.Status == BookingStatus.Pending &&
                b.ExpiresAt.HasValue &&
                b.ExpiresAt.Value < now);

            var list = expiredPending.ToList();
            if (list.Count == 0)
                return 0;

            var count = 0;
            foreach (var booking in list)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var updated = await _unitOfWork.Bookings.UpdateBookingStatusAsync(booking.Id, BookingStatus.Expired);
                if (updated)
                    count++;
            }

            _logger.LogInformation("Expired {Count} pending booking(s) that had passed ExpiresAt", count);
            return count;
        }
    }
}
