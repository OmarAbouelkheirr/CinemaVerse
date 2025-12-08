using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaVerse.Data.Data.Configurations
{
    public class BookingPaymentConfiguration : IEntityTypeConfiguration<BookingPayment>
    {
        public void Configure(EntityTypeBuilder<BookingPayment> builder)
        {
            builder.ToTable("BookingPayments");
            builder.HasKey(bp => bp.Id);

            builder.Property(bp => bp.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            builder.Property(bp => bp.PaymentInentId).HasMaxLength(100);
            builder.Property(bp => bp.Currency)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("EGP");
            builder.Property(bp => bp.TrasnactionDate)
                .IsRequired().HasDefaultValueSql("GETUTCDATE()");

            builder.Property(bp=>bp.Status).HasDefaultValue(PaymentStatus.Pending).HasConversion<int>()
                .IsRequired();

            //relationships configured here
        }
    }
}
