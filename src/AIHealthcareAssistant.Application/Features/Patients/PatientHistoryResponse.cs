using AIHealthcareAssistant.Application.Features.Appointments;

namespace AIHealthcareAssistant.Application.Features.Patients;

public class PatientHistoryResponse
{
    public Guid PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<AppointmentResponse> Appointments { get; set; } = new();
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
}
