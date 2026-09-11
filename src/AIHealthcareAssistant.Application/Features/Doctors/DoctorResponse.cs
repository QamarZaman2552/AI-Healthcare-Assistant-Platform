using AIHealthcareAssistant.Application.Features.Specialties;

namespace AIHealthcareAssistant.Application.Features.Doctors;

public class DoctorResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public string LicenseNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? Biography { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? ClinicName { get; set; }
    public string? ClinicAddress { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }

    public List<SpecialtyResponse> Specialties { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
