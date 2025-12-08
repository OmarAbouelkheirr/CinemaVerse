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
    public class TicketsConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.TicketNumber)
                .IsRequired()
                .HasMaxLength(10);
            builder.Property(t => t.Price).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(t => t.Status).IsRequired().HasDefaultValue(TicketStatus.Active);
            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships can be configured here if needed
        }
    }
}
