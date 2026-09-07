using AIHealthcareAssistant.Domain.Common;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// A recurring weekly window during which a <see cref="Doctor"/> accepts appointments,
/// divided into slots of <see cref="SlotDurationMinutes"/>.
/// </summary>
public class DoctorAvailability : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;

    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
}
