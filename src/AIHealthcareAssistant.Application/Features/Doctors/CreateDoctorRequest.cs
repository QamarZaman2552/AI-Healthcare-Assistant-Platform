namespace AIHealthcareAssistant.Application.Features.Doctors;

/// <summary>
/// Fills in the doctor profile for a user who already has the Doctor role
/// (a bare Doctor row is created automatically at registration).
/// </summary>
public class CreateDoctorRequest
{
    public Guid UserId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? Biography { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? ClinicName { get; set; }
    public string? ClinicAddress { get; set; }
}
