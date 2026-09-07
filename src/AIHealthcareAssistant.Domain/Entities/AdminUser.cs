using AIHealthcareAssistant.Domain.Common;
using AIHealthcareAssistant.Domain.Enums;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// Admin-role projection of a <see cref="User"/>, granting back-office access at a given <see cref="AccessLevel"/>.
/// </summary>
public class AdminUser : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string? Department { get; set; }
    public AdminAccessLevel AccessLevel { get; set; } = AdminAccessLevel.Standard;
    public string? Notes { get; set; }
}
