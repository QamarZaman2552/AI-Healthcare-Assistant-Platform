namespace AIHealthcareAssistant.Application.Features.Appointments;

public class CreateAppointmentRequest
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string? ReasonForVisit { get; set; }
    public string? Notes { get; set; }
}