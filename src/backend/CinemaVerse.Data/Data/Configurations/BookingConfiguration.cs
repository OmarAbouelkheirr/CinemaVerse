using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaVerse.Data.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>   
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.UserId)
                .IsRequired();

            builder.Property(b => b.Status)
                .IsRequired().HasDefaultValue(BookingStatus.Pending).HasConversion<int>();
            builder.Property(b => b.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");

            builder.Property(b => b.CreatedAt).HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

            //relationships configured here 
            builder.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(b => b.MovieShowTime)
                .WithMany(mst => mst.Bookings)
                .HasForeignKey(b => b.MovieShowTimeId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
