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
    public class MovieShowTimeConfiguration : IEntityTypeConfiguration<MovieShowTime>
    {
        public void Configure(EntityTypeBuilder<MovieShowTime> builder)
        {
            builder.HasKey(mst => mst.Id);
            builder.Property(mst => mst.Price).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(mst => mst.ShowStartTime).IsRequired();
            builder.Property(mst => mst.ShowEndTime).IsRequired();//updated to be calculated based on movie duration

            // Relationships can be configured here if needed
        }
    }
}
