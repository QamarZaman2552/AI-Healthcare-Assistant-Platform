namespace AIHealthcareAssistant.Application.Features.Appointments;

public class AppointmentResponse
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string? ReasonForVisit { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid? PatientIntakeId { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? Symptoms { get; set; }

    public Guid? AIConversationId { get; set; }
    public string? AISummary { get; set; }
}