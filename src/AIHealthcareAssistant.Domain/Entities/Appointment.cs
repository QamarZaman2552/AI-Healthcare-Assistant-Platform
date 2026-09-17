using AIHealthcareAssistant.Domain.Common;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// A scheduled consultation between a <see cref="Patient"/> and a <see cref="Doctor"/>.
/// Carries an optional pre-visit <see cref="PatientIntake"/> and an optional linked <see cref="AIConversation"/>.
/// </summary>
public class Appointment : AuditableEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public Guid AppointmentStatusId { get; set; }
    public AppointmentStatus Status { get; set; } = null!;

    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }

    public string? ReasonForVisit { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }

    public Guid? PatientIntakeId { get; set; }
    public PatientIntake? Intake { get; set; }

    public Guid? AIConversationId { get; set; }
    public AIConversation? Conversation { get; set; }
}
