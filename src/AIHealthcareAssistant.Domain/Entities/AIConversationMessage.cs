using AIHealthcareAssistant.Domain.Common;
using AIHealthcareAssistant.Domain.Enums;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// A single turn within an <see cref="AIConversation"/>. <see cref="SequenceNumber"/> preserves order.
/// </summary>
public class AIConversationMessage : BaseEntity
{
    public Guid AIConversationId { get; set; }
    public AIConversation Conversation { get; set; } = null!;

    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? TokenCount { get; set; }
    public int SequenceNumber { get; set; }
}
