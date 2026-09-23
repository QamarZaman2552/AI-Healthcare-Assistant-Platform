using System.ComponentModel.DataAnnotations;
using AIHealthcareAssistant.Domain.Enums;

namespace AIHealthcareAssistant.Application.Features.Patients;

public class PatientProfileRequest
{
    [MaxLength(50)]
    public string? MedicalRecordNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [EnumDataType(typeof(Gender), ErrorMessage = "Gender must be a valid value.")]
    public Gender Gender { get; set; } = Gender.Unspecified;

    [MaxLength(8)]
    public string? BloodGroup { get; set; }

    [Range(30, 300, ErrorMessage = "Height must be between 30 and 300 cm.")]
    public decimal? HeightCm { get; set; }

    [Range(1, 500, ErrorMessage = "Weight must be between 1 and 500 kg.")]
    public decimal? WeightKg { get; set; }

    [MaxLength(200)]
    public string? AddressLine1 { get; set; }

    [MaxLength(200)]
    public string? AddressLine2 { get; set; }

    [MaxLength(100)]

    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(150)]
    public string? EmergencyContactName { get; set; }


    [Phone(ErrorMessage = "Invalid emergency contact phone number.")]
    [MaxLength(30)]

    public string? EmergencyContactPhone { get; set; }

    [MaxLength(1000)]


    public string? Allergies { get; set; }

    [MaxLength(1000)]
    public string? ChronicConditions { get; set; }

    [MaxLength(1000)]
    public string? CurrentMedications { get; set; }
}
