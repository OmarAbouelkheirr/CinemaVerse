using CinemaVerse.Services.Interfaces.Background;
using Microsoft.Extensions.Hosting;

namespace CinemaVerse.BackgroundServices
{
    public class ShowReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ShowReminderBackgroundService> _logger;
        private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(15);

        public ShowReminderBackgroundService(IServiceScopeFactory scopeFactory,ILogger<ShowReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ShowReminder background service started. Running every {Interval} minutes.", RunInterval.TotalMinutes);

            using var timer = new PeriodicTimer(RunInterval);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var remindersService = scope.ServiceProvider.GetRequiredService<IShowReminderService>();
                    var count = await remindersService.SendShowRemindersAsync(stoppingToken);
                    if (count > 0)
                        _logger.LogInformation("ShowReminder: sent {Count} reminder email(s).", count);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ShowReminder background run failed.");
                }
            }

            _logger.LogInformation("ShowReminder background service stopped.");
        }
    }
}
