using AIHealthcareAssistant.Domain.Common;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// Lookup describing the lifecycle state of an <see cref="Appointment"/>
/// (Pending, Confirmed, CheckedIn, Completed, Cancelled, NoShow). Seeded via migration.
/// </summary>
public class AppointmentStatus : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
