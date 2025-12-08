using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CinemaVerse.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaVerse.Data.Data.Configurations
{
    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {

            builder.ToTable("Branchs");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.BranchName).IsRequired().HasMaxLength(50);
            
            builder.Property(b => b.BranchLocation).IsRequired().HasMaxLength(300);

            // ---------------------------------- //

        }
    }
}
