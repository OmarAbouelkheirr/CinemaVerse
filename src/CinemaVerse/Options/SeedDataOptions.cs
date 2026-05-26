namespace CinemaVerse.Options;

public sealed class SeedDataOptions
{
    public const string SectionName = "SeedData";

    // When enabled, seeding runs on application startup after EF migrations.
    public bool Enabled { get; set; } = false;

    // Also require ASPNETCORE_ENVIRONMENT=Development unless this is set true.
    // This is an extra guard so demo data doesn't get seeded accidentally.
    public bool AllowInProduction { get; set; } = false;

    // If true, seed additional sample transactional data (Bookings/Tickets/Payments/Reviews).
    public bool IncludeSampleTransactions { get; set; } = false;

    // Sample user accounts are created if they do not already exist.
    public string AdminEmail { get; set; } = "admin@cinemaverse.local";
    // Passwords should be provided via secrets/env variables, not committed.
    public string AdminPassword { get; set; } = string.Empty;
    public string UserEmail { get; set; } = "user@cinemaverse.local";
    public string UserPassword { get; set; } = string.Empty;

    // Optional: how many future days of showtimes to generate.
    public int ShowtimeDays { get; set; } = 3;

    // Optional: showtimes per hall per day.
    public int ShowtimesPerHallPerDay { get; set; } = 3;
}
