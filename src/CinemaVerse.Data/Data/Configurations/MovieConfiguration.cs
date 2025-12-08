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

            builder.Property(m => m.MovieCast).IsRequired().HasMaxLength(500);

            builder.Property(m => m.TrailerUrl).HasMaxLength(500);

            builder.Property(m => m.MovieDuration).IsRequired();

            builder.Property(m => m.Rating).HasPrecision(2, 1); 
            
            builder.Property(m => m.MovieAgeRating).IsRequired();

            builder.Property(m => m.ReleaseDate).IsRequired();

            // ---------------------------------- //


        }
    }
}
