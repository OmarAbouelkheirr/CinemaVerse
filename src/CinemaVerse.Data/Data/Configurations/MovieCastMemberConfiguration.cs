using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaVerse.Data.Data.Configurations
{
    public class MovieCastMemberConfiguration : IEntityTypeConfiguration<MovieCastMember>
    {
        public void Configure(EntityTypeBuilder<MovieCastMember> builder)
        {
            builder.ToTable("MovieCastMembers");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.PersonName).IsRequired().HasMaxLength(200);
            builder.Property(m => m.ImageUrl).HasMaxLength(500);
            builder.Property(m => m.CharacterName).HasMaxLength(200);

            builder.Property(m => m.RoleType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(m => m.DisplayOrder).HasDefaultValue(0);
            builder.Property(m => m.IsLead).HasDefaultValue(false);

            builder.HasOne(m => m.Movie)
                .WithMany(m => m.CastMembers)
                .HasForeignKey(m => m.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
