using CinemaVerse.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaVerse.Data.Data.Configurations
{
    public class BranchConfiguration : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {

            builder.ToTable("Branches");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.BranchName).IsRequired().HasMaxLength(50);
            
            builder.Property(b => b.BranchLocation).IsRequired().HasMaxLength(300);

            // Relationships configured here


        }
    }
}
