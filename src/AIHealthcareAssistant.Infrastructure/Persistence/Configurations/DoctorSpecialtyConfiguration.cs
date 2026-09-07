using AIHealthcareAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIHealthcareAssistant.Infrastructure.Persistence.Configurations;

public class DoctorSpecialtyConfiguration : IEntityTypeConfiguration<DoctorSpecialty>
{
    public void Configure(EntityTypeBuilder<DoctorSpecialty> builder)
    {
        builder.ToTable("DoctorSpecialties");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsPrimary).HasDefaultValue(false);

        builder.HasIndex(x => new { x.DoctorId, x.SpecialtyId }).IsUnique();
    }
}
