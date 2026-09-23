using System.ComponentModel.DataAnnotations;
using AIHealthcareAssistant.Application.Common.Validation;

namespace AIHealthcareAssistant.Application.Features.Appointments;

public class RescheduleAppointmentRequest
{
    [Required(ErrorMessage = "New scheduled start is required.")]
    [NotDefaultDateTime(ErrorMessage = "New scheduled start must be a valid date/time.")]
    public DateTime NewScheduledStart { get; set; }

    [Required(ErrorMessage = "New scheduled end is required.")]
    [NotDefaultDateTime(ErrorMessage = "New scheduled end must be a valid date/time.")]
    public DateTime NewScheduledEnd { get; set; }
}