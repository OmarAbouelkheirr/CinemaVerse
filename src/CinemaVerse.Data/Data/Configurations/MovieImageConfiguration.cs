using CinemaVerse.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaVerse.Data.Data.Configurations
{
    public class MovieImageConfiguration : IEntityTypeConfiguration<MovieImage>
    {
        public void Configure(EntityTypeBuilder<MovieImage> builder)
        {
            builder.ToTable("MovieImages");

            builder.HasKey(mi => mi.Id);

            builder.Property(mi => mi.ImageUrl).IsRequired().HasMaxLength(300);

            // ---------------------------------- //

        }
    }
}
