using AIHealthcareAssistant.Domain.Common;

namespace AIHealthcareAssistant.Domain.Entities;

/// <summary>
/// Doctor-role projection of a <see cref="User"/>. Linked to specialties through <see cref="DoctorSpecialty"/>,
/// publishes bookable slots via <see cref="DoctorAvailability"/> and is a party to appointments.
/// </summary>
public class Doctor : AuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string LicenseNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? Biography { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? ClinicName { get; set; }
    public string? ClinicAddress { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; }

    public ICollection<DoctorSpecialty> DoctorSpecialties { get; set; } = new List<DoctorSpecialty>();
    public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
