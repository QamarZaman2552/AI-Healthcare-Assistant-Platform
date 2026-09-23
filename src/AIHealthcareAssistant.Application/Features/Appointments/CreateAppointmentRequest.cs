using System.ComponentModel.DataAnnotations;
using AIHealthcareAssistant.Application.Common.Validation;

namespace AIHealthcareAssistant.Application.Features.Appointments;

public class CreateAppointmentRequest
{
    [NotEmptyGuid(ErrorMessage = "PatientId is required.")]
    public Guid PatientId { get; set; }

    [NotEmptyGuid(ErrorMessage = "DoctorId is required.")]
    public Guid DoctorId { get; set; }

    [Required(ErrorMessage = "Scheduled start is required.")]
    [NotDefaultDateTime(ErrorMessage = "Scheduled start must be a valid date/time.")]
    public DateTime ScheduledStart { get; set; }

    [Required(ErrorMessage = "Scheduled end is required.")]
    [NotDefaultDateTime(ErrorMessage = "Scheduled end must be a valid date/time.")]
    public DateTime ScheduledEnd { get; set; }

    [MaxLength(500, ErrorMessage = "Reason for visit cannot exceed 500 characters.")]
    public string? ReasonForVisit { get; set; }

    [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters.")]
    public string? Notes { get; set; }

    public Guid? PatientIntakeId { get; set; }

    public Guid? AIConversationId { get; set; }
}