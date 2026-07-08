using CinemaVerse.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaVerse.Data.Data.Configurations
{
    public class MovieGenreConfiguration : IEntityTypeConfiguration<MovieGenre>
    {
        public void Configure(EntityTypeBuilder<MovieGenre> builder)
        {
            builder.ToTable("MovieGenres");

            builder.HasKey(mg => new {mg.MovieID,mg.GenreID });

            builder.HasOne(mg => mg.Movie)
                   .WithMany(m => m.MovieGenres)
                   .HasForeignKey(mg => mg.MovieID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(mg => mg.Genre)
                     .WithMany(g => g.MovieGenres)
                     .HasForeignKey(mg => mg.GenreID)
                     .OnDelete(DeleteBehavior.Restrict);




        }
    }
}
