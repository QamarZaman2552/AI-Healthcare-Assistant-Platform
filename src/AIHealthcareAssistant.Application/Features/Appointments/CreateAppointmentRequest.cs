using System.ComponentModel.DataAnnotations;

namespace AIHealthcareAssistant.Application.Features.Appointments;

public class CreateAppointmentRequest
{
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }

    [Required(ErrorMessage = "Scheduled start is required.")]
    public DateTime ScheduledStart { get; set; }

    [Required(ErrorMessage = "Scheduled end is required.")]
    public DateTime ScheduledEnd { get; set; }

    [MaxLength(1000, ErrorMessage = "Reason for visit cannot exceed 1000 characters.")]
    public string? ReasonForVisit { get; set; }

    [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters.")]
    public string? Notes { get; set; }

    public Guid? PatientIntakeId { get; set; }

    public Guid? AIConversationId { get; set; }
}