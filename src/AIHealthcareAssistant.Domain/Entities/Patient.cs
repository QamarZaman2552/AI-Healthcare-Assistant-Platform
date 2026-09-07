using AIHealthcareAssistant.Domain.Common;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// Patient-role projection of a <see cref="User"/>. Owns clinical/demographic detail via
/// <see cref="PatientProfile"/> and is the subject of appointments, intakes and AI conversations.
/// </summary>
public class Patient : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string? MedicalRecordNumber { get; set; }

    public PatientProfile? Profile { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<PatientIntake> Intakes { get; set; } = new List<PatientIntake>();
    public ICollection<AIConversation> Conversations { get; set; } = new List<AIConversation>();
}
