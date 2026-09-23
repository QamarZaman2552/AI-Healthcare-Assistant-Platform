using System.ComponentModel.DataAnnotations;

namespace AIHealthcareAssistant.Application.Features.Appointments;

public class CancelAppointmentRequest
{
    [MaxLength(500, ErrorMessage = "Cancellation reason cannot exceed 500 characters.")]
    public string? CancellationReason { get; set; }
}
