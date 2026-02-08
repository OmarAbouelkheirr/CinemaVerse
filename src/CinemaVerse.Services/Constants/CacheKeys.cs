namespace CinemaVerse.Services.Constants;

public static class CacheKeys
{
    public const string AllMovies = "Cache:Movies:All";
    public static string MovieById(int id) => $"Cache:Movies:{id}";
}
