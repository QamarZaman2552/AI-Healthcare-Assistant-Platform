using System.ComponentModel.DataAnnotations;

namespace AIHealthcareAssistant.Application.Features.Appointments;

public class UpdateAppointmentStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
