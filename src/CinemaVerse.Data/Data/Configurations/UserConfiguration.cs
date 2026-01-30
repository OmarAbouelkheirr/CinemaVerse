using CinemaVerse.Data.Enums;
using CinemaVerse.Data.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CinemaVerse.Data.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.PasswordHash)
               .IsRequired()
               .HasMaxLength(512);

            builder.Property(u => u.FirstName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.LastName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            builder.Property(u => u.LastUpdatedAt).HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            builder.Property(u => u.Gender).HasConversion<int>();

            builder.Property(u => u.Role)
                .HasConversion<int>()
                .HasDefaultValue(UserRole.User)
                .IsRequired();

            //relationships configured here 

        }
    }
}
