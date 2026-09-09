namespace AIHealthcareAssistant.Application.Features.Appointments;

public class RescheduleAppointmentRequest
{
    public DateTime NewScheduledStart { get; set; }
    public DateTime NewScheduledEnd { get; set; }
}