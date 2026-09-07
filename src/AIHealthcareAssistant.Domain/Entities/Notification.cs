using AIHealthcareAssistant.Domain.Common;
using AIHealthcareAssistant.Domain.Enums;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// An in-app message addressed to a <see cref="User"/>.
/// </summary>
public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.General;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
}
