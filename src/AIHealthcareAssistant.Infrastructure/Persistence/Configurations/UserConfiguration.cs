using AIHealthcareAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIHealthcareAssistant.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.Role).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.UpdatedBy).HasMaxLength(256);

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Role);

        builder.HasOne(x => x.Patient)
            .WithOne(x => x.User)
            .HasForeignKey<Patient>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Doctor)
            .WithOne(x => x.User)
            .HasForeignKey<Doctor>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AdminUser)
            .WithOne(x => x.User)
            .HasForeignKey<AdminUser>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Notifications)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
