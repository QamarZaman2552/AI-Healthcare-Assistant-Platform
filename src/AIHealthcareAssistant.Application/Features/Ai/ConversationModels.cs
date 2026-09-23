using System.ComponentModel.DataAnnotations;
using AIHealthcareAssistant.Application.Common.Validation;

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
    [NotEmptyGuid(ErrorMessage = "PatientId is required.")]
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
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
    public Guid PatientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public List<ConversationMessageResponse> Messages { get; set; } = new();
}
