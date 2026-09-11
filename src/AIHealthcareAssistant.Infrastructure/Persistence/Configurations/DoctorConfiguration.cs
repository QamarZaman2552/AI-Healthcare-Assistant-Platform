using AIHealthcareAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIHealthcareAssistant.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LicenseNumber).IsRequired().HasMaxLength(64);
        builder.Property(x => x.Biography).HasMaxLength(2000);
        builder.Property(x => x.ConsultationFee).HasPrecision(18, 2);
        builder.Property(x => x.ClinicName).HasMaxLength(200);
        builder.Property(x => x.ClinicAddress).HasMaxLength(300);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsVerified).HasDefaultValue(false);
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.UpdatedBy).HasMaxLength(256);

        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.LicenseNumber).IsUnique();
        builder.HasIndex(x => new { x.IsActive, x.IsVerified });

        builder.HasMany(x => x.DoctorSpecialties)
            .WithOne(x => x.Doctor)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Availabilities)
            .WithOne(x => x.Doctor)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Appointments)
            .WithOne(x => x.Doctor)
            .HasForeignKey(x => x.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
