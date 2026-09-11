namespace AIHealthcareAssistant.Application.Features.PatientIntakes;

public class CreatePatientIntakeRequest
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? AIConversationId { get; set; }
    public Guid? RecommendedSpecialtyId { get; set; }

    public string ChiefComplaint { get; set; } = string.Empty;
    public string? SymptomsDescription { get; set; }
    public string? SymptomOnset { get; set; }
    public int? PainLevel { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public string? BloodPressure { get; set; }
    public int? HeartRateBpm { get; set; }
    public string? CurrentMedications { get; set; }
    public string? AdditionalNotes { get; set; }
    public string? AISummary { get; set; }
}
