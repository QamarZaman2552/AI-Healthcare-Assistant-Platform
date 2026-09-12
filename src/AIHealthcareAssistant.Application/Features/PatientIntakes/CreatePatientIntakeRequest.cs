using System.ComponentModel.DataAnnotations;
namespace AIHealthcareAssistant.Application.Features.PatientIntakes;

public class CreatePatientIntakeRequest
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }
    public Guid? AIConversationId { get; set; }
    public Guid? RecommendedSpecialtyId { get; set; }

    [Required(ErrorMessage = "Chief complaint is required.")]
    [MinLength(2)]
    [MaxLength(1000)]
    public string ChiefComplaint { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? SymptomsDescription { get; set; }

    [MaxLength(200)]

    public string? SymptomOnset { get; set; }

    [Range(0, 10, ErrorMessage = "Pain level must be between 0 and 10.")]
    public int? PainLevel { get; set; }

    [Range(25, 50, ErrorMessage = "Temperature must be between 25°C and 50°C.")]
    public decimal? TemperatureCelsius { get; set; }

    [MaxLength(50)]
    public string? BloodPressure { get; set; }

    [Range(20, 250, ErrorMessage = "Heart rate must be between 20 and 250 BPM.")]

    public int? HeartRateBpm { get; set; }

    [MaxLength(2000)]

    public string? CurrentMedications { get; set; }

    [MaxLength(3000)]
    public string? AdditionalNotes { get; set; }

    [MaxLength(5000)]
    public string? AISummary { get; set; }
}
