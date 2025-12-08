using CinemaVerse.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace CinemaVerse.Data.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {}
        
        public DbSet<ApplicationUser> ApplicationUsers { get; set; } = null!;
        public DbSet<Movie> Movies { get; set; } = null!;
        public DbSet<Genre> Genres { get; set; } = null!;
        public DbSet<Booking> Bookings { get; set; } = null!;
        public DbSet<BookingPayment> BookingPayments { get; set; } = null!;
        public DbSet<Seat> Seats { get; set; } = null!;
        public DbSet<Branch> Branches { get; set; } = null!;
        public DbSet<MovieShowTime> MovieShowTimes { get; set; } = null!;
        public DbSet<Hall> Halls { get; set; } = null!;
        public DbSet<MovieGenre> MovieGenres { get; set; } = null!;
        public DbSet<MovieImage> MovieImages { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

    }
}
