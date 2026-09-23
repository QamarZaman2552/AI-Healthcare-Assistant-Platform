using AIHealthcareAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIHealthcareAssistant.Infrastructure.Persistence.Configurations;

public class PatientIntakeConfiguration : IEntityTypeConfiguration<PatientIntake>
{
    public void Configure(EntityTypeBuilder<PatientIntake> builder)
    {
        builder.ToTable("PatientIntakes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChiefComplaint).IsRequired().HasMaxLength(500);
        builder.Property(x => x.SymptomsDescription).HasMaxLength(2000);
        builder.Property(x => x.SymptomOnset).HasMaxLength(200);
        builder.Property(x => x.TemperatureCelsius).HasPrecision(4, 1);
        builder.Property(x => x.BloodPressure).HasMaxLength(16);
        builder.Property(x => x.CurrentMedications).HasMaxLength(1000);
        builder.Property(x => x.AdditionalNotes).HasMaxLength(1000);
        builder.Property(x => x.AISummary).HasMaxLength(4000);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.SubmittedAt).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.UpdatedBy).HasMaxLength(256);

        builder.HasIndex(x => x.AppointmentId)
            .IsUnique()
            .HasFilter("[AppointmentId] IS NOT NULL AND [IsDeleted] = 0");

        builder.HasIndex(x => x.AIConversationId)
            .IsUnique()
            .HasFilter("[AIConversationId] IS NOT NULL AND [IsDeleted] = 0");

        builder.HasIndex(x => x.PatientId);

        builder.HasOne(x => x.AIConversation)
            .WithMany()
            .HasForeignKey(x => x.AIConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RecommendedSpecialty)
            .WithMany()
            .HasForeignKey(x => x.RecommendedSpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
