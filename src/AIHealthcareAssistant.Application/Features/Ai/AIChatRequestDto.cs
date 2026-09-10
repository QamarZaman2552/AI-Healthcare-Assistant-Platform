using System.ComponentModel.DataAnnotations;

namespace AIHealthcareAssistant.Application.Features.Ai;

public class AIChatRequestDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(4000)]
    public string Message { get; set; } = string.Empty;

    public Guid PatientId { get; set; }

    public Guid? AppointmentId { get; set; }

    public Guid? ConversationId { get; set; }
}