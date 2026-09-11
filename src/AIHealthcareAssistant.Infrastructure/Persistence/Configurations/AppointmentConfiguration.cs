using AIHealthcareAssistant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIHealthcareAssistant.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ScheduledStart).IsRequired();
        builder.Property(x => x.ScheduledEnd).IsRequired();
        builder.Property(x => x.ReasonForVisit).HasMaxLength(500);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.CancellationReason).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.UpdatedBy).HasMaxLength(256);

        builder.HasIndex(x => new { x.DoctorId, x.ScheduledStart });
        builder.HasIndex(x => new { x.PatientId, x.ScheduledStart });
        builder.HasIndex(x => x.AppointmentStatusId);

        builder.HasOne(x => x.Status)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.AppointmentStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Intake)
            .WithOne(x => x.Appointment)
            .HasForeignKey<PatientIntake>(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Conversation)
            .WithOne(x => x.Appointment)
            .HasForeignKey<AIConversation>(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
