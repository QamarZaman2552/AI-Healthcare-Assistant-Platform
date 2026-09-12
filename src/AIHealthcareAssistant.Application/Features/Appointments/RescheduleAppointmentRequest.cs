using System.ComponentModel.DataAnnotations;
namespace AIHealthcareAssistant.Application.Features.Appointments;

public class RescheduleAppointmentRequest
{
    [Required(ErrorMessage = "New scheduled start is required.")]
    public DateTime NewScheduledStart { get; set; }

    [Required(ErrorMessage = "New scheduled end is required.")]
    public DateTime NewScheduledEnd { get; set; }
}