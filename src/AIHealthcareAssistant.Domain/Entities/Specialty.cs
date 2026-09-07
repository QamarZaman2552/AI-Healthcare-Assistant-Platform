using AIHealthcareAssistant.Domain.Common;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// Medical specialty lookup (e.g. Cardiology, Dermatology). Associated with doctors via <see cref="DoctorSpecialty"/>.
/// </summary>
public class Specialty : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<DoctorSpecialty> DoctorSpecialties { get; set; } = new List<DoctorSpecialty>();
}
