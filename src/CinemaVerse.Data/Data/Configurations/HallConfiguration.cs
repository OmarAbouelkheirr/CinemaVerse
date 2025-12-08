using CinemaVerse.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaVerse.Data.Data.Configurations
{
    public class HallConfiguration : IEntityTypeConfiguration<Hall>
    {
        public void Configure(EntityTypeBuilder<Hall> builder)
        {
            builder.ToTable("Halls");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.HallType).IsRequired().HasConversion<int>();

            builder.Property(h => h.HallStatus).IsRequired().HasConversion<int>();

            builder.Property(h => h.HallNumber).IsRequired().HasMaxLength(5);

            builder.Property(h => h.Capacity).IsRequired();

            builder.Property(h => h.BranchId).IsRequired();

            // ---------------------------------- //

        }
    }
}
