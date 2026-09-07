using AIHealthcareAssistant.Domain.Common;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// Structured symptom / vitals questionnaire a patient completes before an <see cref="Appointment"/>.
/// One-to-one with the appointment; also references the <see cref="Patient"/> directly for convenience.
/// </summary>
public class PatientIntake : AuditableEntity
{
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public string ChiefComplaint { get; set; } = string.Empty;
    public string? SymptomsDescription { get; set; }
    public string? SymptomOnset { get; set; }
    public int? PainLevel { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public string? BloodPressure { get; set; }
    public int? HeartRateBpm { get; set; }
    public string? CurrentMedications { get; set; }
    public string? AdditionalNotes { get; set; }
    public DateTime SubmittedAt { get; set; }
}
