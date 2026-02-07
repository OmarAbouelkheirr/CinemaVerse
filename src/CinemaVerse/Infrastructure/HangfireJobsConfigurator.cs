using CinemaVerse.Services.Interfaces.Background;
using Hangfire;

namespace CinemaVerse.Infrastructure;

public static class HangfireJobsConfigurator
{
    public static void ConfigureRecurringJobs()
    {
        // Expire pending bookings every minute
        RecurringJob.AddOrUpdate<IExpirePendingBookingsService>(
            "ExpirePendingBookings",
            service => service.ExpirePendingBookingsAsync(CancellationToken.None),
            Cron.Minutely);

        // Send show reminders every 15 minutes
        RecurringJob.AddOrUpdate<IShowReminderService>(
            "SendShowReminders",
            service => service.SendShowRemindersAsync(CancellationToken.None),
            "*/15 * * * *");
    }
}

