namespace CinemaVerse.Services.Interfaces.Background
{
    public interface IShowReminderService
    {
        Task<int> SendShowRemindersAsync(CancellationToken cancellationToken = default);
    }
}
