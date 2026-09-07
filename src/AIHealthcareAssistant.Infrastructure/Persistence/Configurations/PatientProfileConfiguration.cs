using AIHealthcareAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIHealthcareAssistant.Infrastructure.Persistence.Configurations;

public class PatientProfileConfiguration : IEntityTypeConfiguration<PatientProfile>
{
    public void Configure(EntityTypeBuilder<PatientProfile> builder)
    {
        builder.ToTable("PatientProfiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Gender).HasConversion<int>().IsRequired();
        builder.Property(x => x.BloodGroup).HasMaxLength(8);
        builder.Property(x => x.HeightCm).HasPrecision(5, 2);
        builder.Property(x => x.WeightKg).HasPrecision(5, 2);

        builder.Property(x => x.AddressLine1).HasMaxLength(200);
        builder.Property(x => x.AddressLine2).HasMaxLength(200);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.State).HasMaxLength(100);
        builder.Property(x => x.PostalCode).HasMaxLength(20);
        builder.Property(x => x.Country).HasMaxLength(100);

        builder.Property(x => x.EmergencyContactName).HasMaxLength(150);
        builder.Property(x => x.EmergencyContactPhone).HasMaxLength(32);

        builder.Property(x => x.Allergies).HasMaxLength(1000);
        builder.Property(x => x.ChronicConditions).HasMaxLength(1000);
        builder.Property(x => x.CurrentMedications).HasMaxLength(1000);
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.UpdatedBy).HasMaxLength(256);

        builder.HasIndex(x => x.PatientId).IsUnique();
    }
}
