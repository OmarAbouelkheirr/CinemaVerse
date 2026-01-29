using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaVerse.Data.Data.Configurations
{
    public class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.ToTable("Movies");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.MovieName).IsRequired().HasMaxLength(250);

            builder.Property(m => m.MovieDescription).IsRequired().HasMaxLength(1500);

            builder.Property(m => m.TrailerUrl).HasMaxLength(500);

            builder.Property(m => m.MoviePoster).HasMaxLength(500);

            builder.Property(m => m.MovieDuration).IsRequired();

            builder.Property(m => m.MovieRating).HasPrecision(3, 2); 
            
            builder.Property(m => m.MovieAgeRating)
                .IsRequired()
                .HasConversion<int>(); // Converts enum to int in database

            builder.Property(m => m.ReleaseDate).IsRequired();

            builder.Property(m => m.Status)
                .IsRequired()
                .HasConversion<int>() // Converts enum to int in database
                .HasDefaultValue(MovieStatus.Active); // Default value

        }
    }
}
