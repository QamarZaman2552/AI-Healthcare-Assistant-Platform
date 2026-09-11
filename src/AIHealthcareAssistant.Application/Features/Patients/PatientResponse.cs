namespace AIHealthcareAssistant.Application.Features.Patients;

public class PatientResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public PatientProfileResponse? Profile { get; set; }
    public DateTime CreatedAt { get; set; }
}
