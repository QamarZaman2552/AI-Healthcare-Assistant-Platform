namespace AIHealthcareAssistant.Application.Features.Doctors;

public class DoctorDashboardResponse
{
    public int TotalAppointments { get; set; }
    public int TodayAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public List<AppointmentSummaryResponse> RecentAppointments { get; set; } = new();
}

public class AppointmentSummaryResponse
{
    public Guid Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public string Type { get; set; } = string.Empty;
}
