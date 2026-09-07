using AIHealthcareAssistant.Domain.Common;
using AIHealthcareAssistant.Domain.Enums;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// A chat session between a <see cref="Patient"/> and the AI assistant. Optionally tied to an
/// <see cref="Appointment"/>. Holds an ordered list of <see cref="AIConversationMessage"/>.
/// </summary>
public class AIConversation : AuditableEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public string Title { get; set; } = string.Empty;
    public ConversationStatus Status { get; set; } = ConversationStatus.Active;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? Summary { get; set; }

    public ICollection<AIConversationMessage> Messages { get; set; } = new List<AIConversationMessage>();
}
