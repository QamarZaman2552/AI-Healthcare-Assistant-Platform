using System.ComponentModel.DataAnnotations;
using AIHealthcareAssistant.Application.Common.Validation;

namespace AIHealthcareAssistant.Application.Features.Doctors;

/// <summary>
/// Creates the doctor profile for a user who already has the Doctor role.
/// Registration with role=Doctor only creates the User; this endpoint
/// creates the Doctor row.
/// </summary>
public class CreateDoctorRequest
{
    [NotEmptyGuid(ErrorMessage = "UserId is required.")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "License number is required.")]
    [MinLength(2)]
    [MaxLength(64)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Range(0, 70, ErrorMessage = "Years of experience must be between 0 and 70.")]
    public int YearsOfExperience { get; set; }

    [MaxLength(2000)]
    public string? Biography { get; set; }

    [Range(0, 1000000, ErrorMessage = "Consultation fee must be between 0 and 1,000,000.")]
    public decimal ConsultationFee { get; set; }

    [MaxLength(200)]
    public string? ClinicName { get; set; }
    [MaxLength(300)]
    public string? ClinicAddress { get; set; }
}
