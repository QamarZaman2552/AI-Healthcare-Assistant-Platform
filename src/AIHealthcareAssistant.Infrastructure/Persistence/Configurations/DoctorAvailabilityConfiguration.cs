using AIHealthcareAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIHealthcareAssistant.Infrastructure.Persistence.Configurations;

public class DoctorAvailabilityConfiguration : IEntityTypeConfiguration<DoctorAvailability>
{
    public void Configure(EntityTypeBuilder<DoctorAvailability> builder)
    {
        builder.ToTable("DoctorAvailabilities");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DayOfWeek).HasConversion<int>().IsRequired();
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();
        builder.Property(x => x.SlotDurationMinutes).HasDefaultValue(30);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => new { x.DoctorId, x.DayOfWeek });
    }
}
