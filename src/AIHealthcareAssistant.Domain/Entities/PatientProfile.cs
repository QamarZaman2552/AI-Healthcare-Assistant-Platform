using AIHealthcareAssistant.Domain.Common;
using AIHealthcareAssistant.Domain.Enums;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// One-to-one extension of <see cref="Patient"/> holding demographic, contact and baseline medical data.
/// </summary>
public class PatientProfile : AuditableEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }
    public Gender Gender { get; set; } = Gender.Unspecified;
    public string? BloodGroup { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? CurrentMedications { get; set; }
}
