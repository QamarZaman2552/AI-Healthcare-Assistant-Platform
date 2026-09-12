using System.ComponentModel.DataAnnotations;
namespace AIHealthcareAssistant.Application.Features.Doctors;

public class UpdateDoctorRequest
{
    [Required(ErrorMessage = "License number is required.")]
    [MinLength(2)]
    [MaxLength(100)]
    public string LicenseNumber { get ;set; } = string.Empty;

    [Range(0, 70)]
    public int YearsOfExperience { get; set; }

    [MaxLength(3000)]
    public string? Biography { get; set; }

    [Range(0, 1000000)]
    public decimal ConsultationFee { get; set; }

    [MaxLength(200)]
    public string? ClinicName { get; set; }

    [MaxLength(500)]
    public string? ClinicAddress { get; set; }
}
