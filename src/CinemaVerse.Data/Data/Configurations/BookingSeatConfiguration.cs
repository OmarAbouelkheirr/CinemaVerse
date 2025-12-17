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
    public class BookingSeatConfiguration : IEntityTypeConfiguration<BookingSeat>
    {
        public void Configure(EntityTypeBuilder<BookingSeat> builder)
        {
            builder.HasKey(bs => new { bs.BookingId, bs.SeatId });

            builder.HasOne(bs => bs.Booking)
                  .WithMany(b => b.BookingSeats)
                  .HasForeignKey(bs => bs.BookingId);

            builder.HasOne(bs => bs.Seat)
                  .WithMany(s => s.BookingSeats)
                  .HasForeignKey(bs => bs.SeatId);
        }
    }
}
