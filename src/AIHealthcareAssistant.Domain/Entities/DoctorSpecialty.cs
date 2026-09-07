using AIHealthcareAssistant.Domain.Common;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// Join entity resolving the many-to-many relationship between <see cref="Doctor"/> and <see cref="Specialty"/>.
/// </summary>
public class DoctorSpecialty : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public Guid SpecialtyId { get; set; }
    public Specialty Specialty { get; set; } = null!;

    public bool IsPrimary { get; set; }
}
