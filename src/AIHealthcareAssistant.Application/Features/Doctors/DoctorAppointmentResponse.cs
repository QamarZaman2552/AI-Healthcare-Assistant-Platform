namespace AIHealthcareAssistant.Application.Features.Doctors;

public class DoctorAppointmentResponse
{
    public Guid Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}
