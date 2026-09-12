using System.ComponentModel.DataAnnotations;
namespace AIHealthcareAssistant.Application.Features.Doctors;

/// <summary>
/// Fills in the doctor profile for a user who already has the Doctor role
/// (a bare Doctor row is created automatically at registration).
/// </summary>
public class CreateDoctorRequest
{
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "License number is required.")]
    [MinLength(2)]
    [MaxLength(100)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Range(0, 70, ErrorMessage = "Years of experience must be between 0 and 70.")]
    public int YearsOfExperience { get; set; }

    [MaxLength(3000)]
    public string? Biography { get; set; }

    [Range(0, 1000000, ErrorMessage = "Consultation fee must be between 0 and 1,000,000.")]
    public decimal ConsultationFee { get; set; }

    [MaxLength(200)]
    public string? ClinicName { get; set; }
    [MaxLength(500)]
    public string? ClinicAddress { get; set; }
}
