using AIHealthcareAssistant.Domain.Common;
using AIHealthcareAssistant.Domain.Enums;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// Root identity record. Every person in the system (patient, doctor, admin) has exactly one User.
/// The role-specific profile is held in <see cref="Patient"/>, <see cref="Doctor"/> or <see cref="AdminUser"/>.
/// </summary>
public class User : AuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public AdminUser? AdminUser { get; set; }
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
