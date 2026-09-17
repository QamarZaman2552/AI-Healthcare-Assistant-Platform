using System.ComponentModel.DataAnnotations;

namespace AIHealthcareAssistant.Application.Features.Ai;

public class AIChatRequestDto
{
    [Required(ErrorMessage = "Message is required.")]
    [MinLength(1, ErrorMessage = "Message cannot be empty.")]
    [MaxLength(4000, ErrorMessage = "Message cannot exceed 4000 characters.")]

    public string Message { get; set; } = string.Empty;

    public Guid? PatientId { get; set; }

    public Guid? AppointmentId { get; set; }

    public Guid? ConversationId { get; set; }
}