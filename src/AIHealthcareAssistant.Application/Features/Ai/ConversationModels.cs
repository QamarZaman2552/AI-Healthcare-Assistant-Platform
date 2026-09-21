namespace AIHealthcareAssistant.Application.Features.Ai;

public class ConversationResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public int MessageCount { get; set; }
    public DateTime? EndedAt { get; set; }
}

public class CreateConversationRequest
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string Title { get; set; } = string.Empty;
}

public class ConversationMessageResponse
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ConversationDetailResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public List<ConversationMessageResponse> Messages { get; set; } = new();
}
