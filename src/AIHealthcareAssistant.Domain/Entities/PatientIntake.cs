using AIHealthcareAssistant.Domain.Common;
using AIHealthcareAssistant.Domain.Enums;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// Structured symptom / vitals questionnaire a patient completes, typically produced from an
/// <see cref="AIConversation"/>. Captured before an <see cref="Appointment"/> exists, so the
/// appointment link is optional and is filled in once the patient books.
/// </summary>
public class PatientIntake : AuditableEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;


    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public Guid? AIConversationId { get; set; }
    public AIConversation? AIConversation { get; set; }

    public Guid? RecommendedSpecialtyId { get; set; }
    public Specialty? RecommendedSpecialty { get; set; }

    public string ChiefComplaint { get; set; } = string.Empty;
    public string? SymptomsDescription { get; set; }
    public string Symptoms => SymptomsDescription ?? ChiefComplaint;
    public string? SymptomOnset { get; set; }
    public int? PainLevel { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public string? BloodPressure { get; set; }
    public int? HeartRateBpm { get; set; }
    public string? CurrentMedications { get; set; }
    public string? AdditionalNotes { get; set; }

    /// <summary>AI-generated structured summary of the conversation/intake.</summary>
    public string? AISummary { get; set; }

    public IntakeStatus Status { get; set; } = IntakeStatus.Submitted;
    public DateTime SubmittedAt { get; set; }
}
