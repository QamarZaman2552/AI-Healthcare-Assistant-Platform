using System.ComponentModel.DataAnnotations;
namespace AIHealthcareAssistant.Application.Features.Availability;

public class CreateAvailabilityRequest
{
    public Guid DoctorId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }

    [Required(ErrorMessage = "Start time is required.")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "End time is required.")]
    public TimeOnly EndTime { get; set; }

    [Range(5, 240, ErrorMessage = "Slot duration must be between 5 and 240 minutes.")]
    public int SlotDurationMinutes { get; set; } = 30;
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}