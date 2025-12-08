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
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Status)
                .IsRequired().HasDefaultValue(BookingStatus.Pending);
            builder.Property(b => b.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");

            builder.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

            //relationships configured here 
        }
    }
}
