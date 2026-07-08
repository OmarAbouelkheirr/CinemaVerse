using System.Security.Cryptography;
using CinemaVerse.Data.Data;
using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using CinemaVerse.Data.Models.Users;
using CinemaVerse.Services.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CinemaVerse.Options;

namespace CinemaVerse.Infrastructure;

// Seed is designed to be safe to run multiple times (idempotent).
public sealed class DatabaseSeeder
{
    private readonly AppDbContext _db;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IHostEnvironment _env;
    private readonly SeedDataOptions _options;

    public DatabaseSeeder(AppDbContext db, ILogger<DatabaseSeeder> logger, IHostEnvironment env, IOptions<SeedDataOptions> options)
    {
        _db = db;
        _logger = logger;
        _env = env;
        _options = options.Value;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        // Extra safety: even if called, refuse to seed in production unless explicitly allowed.
        if (_env.IsProduction() && !_options.AllowInProduction)
        {
            _logger.LogWarning("Database seeding is enabled but blocked in Production. Set SeedData:AllowInProduction=true to override.");
            return;
        }

        _logger.LogInformation(
            "Seeding database (IncludeSampleTransactions={IncludeSampleTransactions}, Environment={Environment})",
            _options.IncludeSampleTransactions, _env.EnvironmentName);

        // Seed order matters due to FK constraints.
        await SeedUsersAsync(cancellationToken);
        await SeedCatalogAsync(cancellationToken);
        await SeedBranchesAndHallsAsync(cancellationToken);
        await SeedShowtimesAsync(cancellationToken);

        if (_options.IncludeSampleTransactions)
            await SeedSampleTransactionsAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var adminEmail = (_options.AdminEmail ?? string.Empty).Trim().ToLowerInvariant();
        var userEmail = (_options.UserEmail ?? string.Empty).Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(adminEmail))
        {
            var exists = await _db.Users.AnyAsync(u => u.Email == adminEmail, cancellationToken);
            if (!exists)
            {
                var password = await ResolvePasswordAsync("SeedData:AdminPassword", _options.AdminPassword, cancellationToken);
                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Admin user seed skipped because no password was provided (use env var SeedData__AdminPassword or config SeedData:AdminPassword). Email={Email}", adminEmail);
                }
                else
                {
                _db.Users.Add(new User
                {
                    Email = adminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    FirstName = "System",
                    LastName = "Admin",
                    PhoneNumber = null,
                    Address = "N/A",
                    City = "N/A",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = Genders.Male,
                    IsActive = true,
                    IsEmailConfirmed = true,
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                });
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            var exists = await _db.Users.AnyAsync(u => u.Email == userEmail, cancellationToken);
            if (!exists)
            {
                var password = await ResolvePasswordAsync("SeedData:UserPassword", _options.UserPassword, cancellationToken);
                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("User seed skipped because no password was provided (use env var SeedData__UserPassword or config SeedData:UserPassword). Email={Email}", userEmail);
                }
                else
                {
                _db.Users.Add(new User
                {
                    Email = userEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    FirstName = "Demo",
                    LastName = "User",
                    PhoneNumber = null,
                    Address = "N/A",
                    City = "N/A",
                    DateOfBirth = new DateTime(1998, 1, 1),
                    Gender = Genders.Female,
                    IsActive = true,
                    IsEmailConfirmed = true,
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                });
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCatalogAsync(CancellationToken cancellationToken)
    {
        // Genres
        var genreNames = new[]
        {
            "Action", "Adventure", "Animation", "Comedy", "Crime",
            "Drama", "Family", "Fantasy", "Horror", "Romance",
            "Sci-Fi", "Thriller"
        };

        foreach (var name in genreNames)
        {
            var exists = await _db.Genres.AnyAsync(g => g.GenreName == name, cancellationToken);
            if (!exists)
                _db.Genres.Add(new Genre { GenreName = name });
        }
        await _db.SaveChangesAsync(cancellationToken);

        // Movies
        if (!await _db.Movies.AnyAsync(cancellationToken))
        {
            _db.Movies.AddRange(
                new Movie
                {
                    MovieName = "Nebula Drift",
                    MovieDescription = "A deep-space rescue mission goes wrong when the crew discovers an impossible signal.",
                    MovieDuration = TimeSpan.FromMinutes(118),
                    MovieRating = 4.20m,
                    MovieAgeRating = MovieAgeRating.PG13,
                    ReleaseDate = new DateOnly(2025, 10, 10),
                    TrailerUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    MoviePoster = "/uploads/posters/nebula-drift.jpg",
                    Language = "English",
                    Status = MovieStatus.Active
                },
                new Movie
                {
                    MovieName = "Cairo Nights",
                    MovieDescription = "Two strangers cross paths in the city and unravel a shared past over one night.",
                    MovieDuration = TimeSpan.FromMinutes(105),
                    MovieRating = 3.80m,
                    MovieAgeRating = MovieAgeRating.PG,
                    ReleaseDate = new DateOnly(2024, 5, 20),
                    TrailerUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    MoviePoster = "/uploads/posters/cairo-nights.jpg",
                    Language = "Arabic",
                    Status = MovieStatus.Active
                },
                new Movie
                {
                    MovieName = "The Last Laugh",
                    MovieDescription = "A comedian's final tour turns into a mystery when jokes start coming true.",
                    MovieDuration = TimeSpan.FromMinutes(99),
                    MovieRating = 3.50m,
                    MovieAgeRating = MovieAgeRating.PG13,
                    ReleaseDate = new DateOnly(2023, 11, 2),
                    TrailerUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                    MoviePoster = "/uploads/posters/the-last-laugh.jpg",
                    Language = "English",
                    Status = MovieStatus.Active
                }
            );

            await _db.SaveChangesAsync(cancellationToken);
        }

        // Images and cast members (helps frontend screens without admin uploads).
        await SeedMovieMediaAsync(cancellationToken);

        // MovieGenres (simple mapping by name)
        var movies = await _db.Movies.AsNoTracking().ToListAsync(cancellationToken);
        var genres = await _db.Genres.AsNoTracking().ToListAsync(cancellationToken);

        var action = genres.FirstOrDefault(g => g.GenreName == "Action");
        var drama = genres.FirstOrDefault(g => g.GenreName == "Drama");
        var romance = genres.FirstOrDefault(g => g.GenreName == "Romance");
        var comedy = genres.FirstOrDefault(g => g.GenreName == "Comedy");
        var thriller = genres.FirstOrDefault(g => g.GenreName == "Thriller");
        var sciFi = genres.FirstOrDefault(g => g.GenreName == "Sci-Fi");

        foreach (var movie in movies)
        {
            if (action != null && movie.MovieName == "Nebula Drift")
                await EnsureMovieGenreAsync(movie.Id, action.Id, cancellationToken);
            if (sciFi != null && movie.MovieName == "Nebula Drift")
                await EnsureMovieGenreAsync(movie.Id, sciFi.Id, cancellationToken);

            if (drama != null && movie.MovieName == "Cairo Nights")
                await EnsureMovieGenreAsync(movie.Id, drama.Id, cancellationToken);
            if (romance != null && movie.MovieName == "Cairo Nights")
                await EnsureMovieGenreAsync(movie.Id, romance.Id, cancellationToken);

            if (comedy != null && movie.MovieName == "The Last Laugh")
                await EnsureMovieGenreAsync(movie.Id, comedy.Id, cancellationToken);
            if (thriller != null && movie.MovieName == "The Last Laugh")
                await EnsureMovieGenreAsync(movie.Id, thriller.Id, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureMovieGenreAsync(int movieId, int genreId, CancellationToken cancellationToken)
    {
        var exists = await _db.MovieGenres.AnyAsync(mg => mg.MovieID == movieId && mg.GenreID == genreId, cancellationToken);
        if (!exists)
            _db.MovieGenres.Add(new MovieGenre { MovieID = movieId, GenreID = genreId });
    }

    private async Task SeedBranchesAndHallsAsync(CancellationToken cancellationToken)
    {
        // Branches
        if (!await _db.Branches.AnyAsync(cancellationToken))
        {
            _db.Branches.AddRange(
                new Branch { BranchName = "CinemaVerse Downtown", BranchLocation = "Downtown" },
                new Branch { BranchName = "CinemaVerse Mall", BranchLocation = "City Mall" }
            );
            await _db.SaveChangesAsync(cancellationToken);
        }

        // Halls (ensure at least one hall per branch)
        var branches = await _db.Branches.AsNoTracking().ToListAsync(cancellationToken);
        foreach (var branch in branches)
        {
            var hasHalls = await _db.Halls.AnyAsync(h => h.BranchId == branch.Id, cancellationToken);
            if (hasHalls) continue;

            // Keep hall numbers small (max length 5)
            var hall1 = new Hall
            {
                BranchId = branch.Id,
                HallNumber = "H1",
                HallStatus = HallStatus.Available,
                HallType = HallType.TwoD,
                Capacity = HallTypeLayoutConfig.GetCapacity(HallType.TwoD)
            };
            var hall2 = new Hall
            {
                BranchId = branch.Id,
                HallNumber = "H2",
                HallStatus = HallStatus.Available,
                HallType = HallType.ThreeD,
                Capacity = HallTypeLayoutConfig.GetCapacity(HallType.ThreeD)
            };

            _db.Halls.AddRange(hall1, hall2);
            await _db.SaveChangesAsync(cancellationToken);

            // Seats: generate via shared layout config (same as admin hall creation).
            await EnsureSeatsForHallAsync(hall1.Id, hall1.HallType, cancellationToken);
            await EnsureSeatsForHallAsync(hall2.Id, hall2.HallType, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task EnsureSeatsForHallAsync(int hallId, HallType hallType, CancellationToken cancellationToken)
    {
        var hasSeats = await _db.Seats.AnyAsync(s => s.HallId == hallId, cancellationToken);
        if (hasSeats) return;

        var layout = HallTypeLayoutConfig.GetLayout(hallType);
        if (layout.NumberOfRows <= 0 || layout.SeatColumns.Count == 0) return;

        char rowLetter = 'A';
        for (int row = 0; row < layout.NumberOfRows; row++)
        {
            foreach (var col in layout.SeatColumns)
            {
                _db.Seats.Add(new Seat
                {
                    HallId = hallId,
                    SeatLabel = $"{rowLetter}{col}"
                });
            }
            rowLetter++;
        }
    }

    private async Task SeedShowtimesAsync(CancellationToken cancellationToken)
    {
        // Only add showtimes if none exist.
        if (await _db.MovieShowTimes.AnyAsync(cancellationToken))
            return;

        var movies = await _db.Movies.AsNoTracking().ToListAsync(cancellationToken);
        var halls = await _db.Halls.AsNoTracking().ToListAsync(cancellationToken);

        if (movies.Count == 0 || halls.Count == 0)
            return;

        // Create future showtimes per hall with non-overlapping blocks.
        var startDate = DateTime.UtcNow.Date.AddDays(1).AddHours(12); // tomorrow 12:00 UTC
        var prices = new[] { 120m, 150m, 180m };
        int priceIdx = 0;

        var days = Math.Clamp(_options.ShowtimeDays, 1, 30);
        var perDay = Math.Clamp(_options.ShowtimesPerHallPerDay, 1, 6);

        foreach (var hall in halls)
        {
            for (int day = 0; day < days; day++)
            {
                var cursor = startDate.AddDays(day);
                for (int i = 0; i < perDay; i++)
                {
                    var movie = movies[(hall.Id + day + i) % movies.Count];
                    var end = cursor.Add(movie.MovieDuration);

                    _db.MovieShowTimes.Add(new MovieShowTime
                    {
                        HallId = hall.Id,
                        MovieId = movie.Id,
                        ShowStartTime = cursor,
                        ShowEndTime = end,
                        Price = prices[priceIdx % prices.Length],
                        CreatedAt = DateTime.UtcNow
                    });

                    priceIdx++;
                    cursor = end.AddMinutes(30); // buffer
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSampleTransactionsAsync(CancellationToken cancellationToken)
    {
        // Guard: if we already have bookings, don't add sample transactional data.
        if (await _db.Bookings.AnyAsync(cancellationToken))
            return;

        var userEmail = (_options.UserEmail ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(userEmail))
            return;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);
        if (user == null)
            return;

        var showtime = await _db.MovieShowTimes
            .AsNoTracking()
            .OrderBy(mst => mst.ShowStartTime)
            .FirstOrDefaultAsync(cancellationToken);
        if (showtime == null)
            return;

        var seatIds = await _db.Seats
            .Where(s => s.HallId == showtime.HallId)
            .OrderBy(s => s.Id)
            .Select(s => s.Id)
            .Take(2)
            .ToListAsync(cancellationToken);
        if (seatIds.Count == 0)
            return;

        var booking = new Booking
        {
            UserId = user.Id,
            MovieShowTimeId = showtime.Id,
            Status = BookingStatus.Confirmed,
            TotalAmount = showtime.Price * seatIds.Count,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = null
        };
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var seatId in seatIds)
        {
            _db.BookingSeat.Add(new BookingSeat { BookingId = booking.Id, SeatId = seatId });
            _db.Tickets.Add(new Ticket
            {
                BookingId = booking.Id,
                SeatId = seatId,
                Price = showtime.Price,
                Status = TicketStatus.Active,
                TicketNumber = GenerateTicketNumber(),
                QrToken = GenerateQrToken()
            });
        }

        _db.BookingPayments.Add(new BookingPayment
        {
            BookingId = booking.Id,
            Amount = booking.TotalAmount,
            Currency = "USD",
            PaymentIntentId = $"seed_pi_{Guid.NewGuid():N}",
            Status = PaymentStatus.Completed,
            TransactionDate = DateTime.UtcNow
        });

        // Optional review for the movie.
        var alreadyReviewed = await _db.Reviews.AnyAsync(r => r.UserId == user.Id && r.MovieId == showtime.MovieId, cancellationToken);
        if (!alreadyReviewed)
        {
            _db.Reviews.Add(new Review
            {
                UserId = user.Id,
                MovieId = showtime.MovieId,
                Rating = 4.00m,
                Comment = "Seeded sample review.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded sample booking/tickets/payments for user {Email}", userEmail);
    }

    private async Task SeedMovieMediaAsync(CancellationToken cancellationToken)
    {
        var movies = await _db.Movies.AsNoTracking().ToListAsync(cancellationToken);
        if (movies.Count == 0)
            return;

        foreach (var movie in movies)
        {
            // A couple of extra images per movie.
            for (int i = 1; i <= 2; i++)
            {
                var url = $"/uploads/movies/{movie.Id}/image-{i}.jpg";
                var exists = await _db.MovieImages.AnyAsync(mi => mi.MovieId == movie.Id && mi.ImageUrl == url, cancellationToken);
                if (!exists)
                    _db.MovieImages.Add(new MovieImage { MovieId = movie.Id, ImageUrl = url });
            }

            // Basic cast.
            await EnsureCastMemberAsync(movie.Id, "Alex Morgan", CastRoleType.Actor, "Lead", 0, true, cancellationToken);
            await EnsureCastMemberAsync(movie.Id, "Samira Hassan", CastRoleType.Actor, "Supporting", 1, false, cancellationToken);
            await EnsureCastMemberAsync(movie.Id, "Jordan Lee", CastRoleType.Director, null, 2, false, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCastMemberAsync(
        int movieId,
        string personName,
        CastRoleType roleType,
        string? characterName,
        int displayOrder,
        bool isLead,
        CancellationToken cancellationToken)
    {
        var exists = await _db.MovieCastMembers.AnyAsync(
            cm => cm.MovieId == movieId && cm.PersonName == personName && cm.RoleType == roleType,
            cancellationToken);

        if (exists) return;

        _db.MovieCastMembers.Add(new MovieCastMember
        {
            MovieId = movieId,
            PersonName = personName,
            RoleType = roleType,
            CharacterName = characterName,
            DisplayOrder = displayOrder,
            IsLead = isLead,
            ImageUrl = $"/uploads/cast/{personName.Replace(' ', '-').ToLowerInvariant()}.jpg"
        });
    }

    private Task<string?> ResolvePasswordAsync(string configKey, string configuredValue, CancellationToken cancellationToken)
    {
        // Do not log the secret value.
        // NOTE: configuration already includes env-var overrides, so if this value is empty
        // then the secret wasn't provided.
        _ = configKey;
        _ = cancellationToken;
        return Task.FromResult<string?>(string.IsNullOrWhiteSpace(configuredValue) ? null : configuredValue);
    }

    private static string GenerateTicketNumber()
    {
        // 10 characters max as per configuration.
        // Use 8 digits + 2 letters: e.g. 48201937AB
        var digits = RandomNumberGenerator.GetInt32(10000000, 99999999);
        const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var a = letters[RandomNumberGenerator.GetInt32(0, letters.Length)];
        var b = letters[RandomNumberGenerator.GetInt32(0, letters.Length)];
        return $"{digits}{a}{b}";
    }

    private static string GenerateQrToken()
    {
        // Configuration allows up to 128.
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
