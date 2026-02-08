namespace CinemaVerse.Services.Constants;

public class CachingOptions
{
    public const string SectionName = "Caching";
    public bool EnableCaching { get; set; } = true;
    public int Minutes { get; set; } = 10;
}
