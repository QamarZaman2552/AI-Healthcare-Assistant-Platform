namespace AIHealthcareAssistant.Application.Features.Patients;

public class PatientDashboardResponse
{
    public int TotalAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public List<RecentActivityResponse> RecentActivity { get; set; } = new();
}

public class RecentActivityResponse
{
    public Guid AppointmentId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public string Type { get; set; } = string.Empty;
}
