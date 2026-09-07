using AIHealthcareAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIHealthcareAssistant.Infrastructure.Persistence.Configurations;

public class AppointmentStatusConfiguration : IEntityTypeConfiguration<AppointmentStatus>
{
    // Stable identifiers so the seed rows are deterministic across migrations.
    public static readonly Guid PendingId = Guid.Parse("11111111-1111-1111-1111-111111111101");
    public static readonly Guid ConfirmedId = Guid.Parse("11111111-1111-1111-1111-111111111102");
    public static readonly Guid CheckedInId = Guid.Parse("11111111-1111-1111-1111-111111111103");
    public static readonly Guid CompletedId = Guid.Parse("11111111-1111-1111-1111-111111111104");
    public static readonly Guid CancelledId = Guid.Parse("11111111-1111-1111-1111-111111111105");
    public static readonly Guid NoShowId = Guid.Parse("11111111-1111-1111-1111-111111111106");

    private static readonly DateTime SeedTimestamp = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<AppointmentStatus> builder)
    {
        builder.ToTable("AppointmentStatuses");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(200);

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasData(
            new AppointmentStatus { Id = PendingId, Name = "Pending", Description = "Requested, awaiting confirmation", CreatedAt = SeedTimestamp },
            new AppointmentStatus { Id = ConfirmedId, Name = "Confirmed", Description = "Confirmed by the clinic", CreatedAt = SeedTimestamp },
            new AppointmentStatus { Id = CheckedInId, Name = "CheckedIn", Description = "Patient has checked in", CreatedAt = SeedTimestamp },
            new AppointmentStatus { Id = CompletedId, Name = "Completed", Description = "Consultation finished", CreatedAt = SeedTimestamp },
            new AppointmentStatus { Id = CancelledId, Name = "Cancelled", Description = "Cancelled by patient or clinic", CreatedAt = SeedTimestamp },
            new AppointmentStatus { Id = NoShowId, Name = "NoShow", Description = "Patient did not attend", CreatedAt = SeedTimestamp });
    }
}
