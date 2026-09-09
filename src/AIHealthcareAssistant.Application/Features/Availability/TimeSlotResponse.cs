namespace AIHealthcareAssistant.Application.Features.Availability;

public class TimeSlotResponse
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; }
}