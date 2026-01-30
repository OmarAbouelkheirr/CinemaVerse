using CinemaVerse.Services.Interfaces.Background;
using Microsoft.Extensions.Hosting;

namespace CinemaVerse.BackgroundServices
{

    public class ExpirePendingBookingsBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpirePendingBookingsBackgroundService> _logger;
        private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(1);

        public ExpirePendingBookingsBackgroundService(IServiceScopeFactory scopeFactory,ILogger<ExpirePendingBookingsBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpirePendingBookings background service started. Running every {Interval} minutes.", RunInterval.TotalMinutes);

            using var timer = new PeriodicTimer(RunInterval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var expireService = scope.ServiceProvider.GetRequiredService<IExpirePendingBookingsService>();
                    var count = await expireService.ExpirePendingBookingsAsync(stoppingToken);
                    if (count > 0)
                        _logger.LogInformation("ExpirePendingBookings: marked {Count} booking(s) as expired.", count);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ExpirePendingBookings background run failed.");
                }
            }

            _logger.LogInformation("ExpirePendingBookings background service stopped.");
        }
    }
}
