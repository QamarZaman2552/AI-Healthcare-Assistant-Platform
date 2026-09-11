namespace AIHealthcareAssistant.Application.Features.Doctors;

public class UpdateDoctorRequest
{
    public string LicenseNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? Biography { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? ClinicName { get; set; }
    public string? ClinicAddress { get; set; }
}
