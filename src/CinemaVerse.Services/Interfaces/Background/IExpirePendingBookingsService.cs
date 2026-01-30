namespace CinemaVerse.Services.Interfaces.Background
{
    public interface IExpirePendingBookingsService
    {
        Task<int> ExpirePendingBookingsAsync(CancellationToken cancellationToken = default);
    }
}
