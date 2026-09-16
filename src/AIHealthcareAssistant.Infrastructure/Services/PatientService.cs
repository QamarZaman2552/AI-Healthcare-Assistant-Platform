using AIHealthcareAssistant.Application.Common.Validation;
using AIHealthcareAssistant.Application.Features.Patients;
using AIHealthcareAssistant.Domain.Entities;
using AIHealthcareAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIHealthcareAssistant.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly AppDbContext _context;

    public PatientService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PatientResponse?> GetByIdAsync(Guid id)
    {
        var patient = await BaseQuery().FirstOrDefaultAsync(p => p.Id == id);
        return patient == null ? null : ToResponse(patient);
    }

    public async Task<PatientResponse?> GetByUserIdAsync(Guid userId)
    {
        var patient = await BaseQuery().FirstOrDefaultAsync(p => p.UserId == userId);
        return patient == null ? null : ToResponse(patient);
    }

    public async Task<List<PatientResponse>> GetAllAsync()
    {
        var patients = await BaseQuery()
            .OrderBy(p => p.User.FirstName)
            .ThenBy(p => p.User.LastName)
            .ToListAsync();

        return patients.Select(ToResponse).ToList();
    }

    public async Task<PatientResponse> RegisterAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        if (await _context.Patients.AnyAsync(p => p.UserId == userId))
            throw new InvalidOperationException("Patient is already registered");

        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        return ToResponse(patient);
    }

    public async Task<PatientProfileResponse?> GetProfileAsync(Guid patientId)
    {
        var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.PatientId == patientId);
        return profile == null ? null : ToProfileResponse(profile);
    }

    public async Task<PatientProfileResponse> CreateProfileAsync(Guid patientId, PatientProfileRequest request)
    {
        var patient = await _context.Patients
            .Include(p => p.Profile)
            .FirstOrDefaultAsync(p => p.Id == patientId)
            ?? throw new KeyNotFoundException("Patient not found");

        if (patient.Profile != null)
            throw new InvalidOperationException("A profile already exists for this patient. Use update instead.");

        ValidateProfile(request);

        patient.MedicalRecordNumber = request.MedicalRecordNumber ?? patient.MedicalRecordNumber;

        var profile = new PatientProfile { Id = Guid.NewGuid(), PatientId = patientId };
        ApplyProfile(profile, request);

        _context.PatientProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return ToProfileResponse(profile);
    }

    public async Task<PatientProfileResponse> UpdateProfileAsync(Guid patientId, PatientProfileRequest request)
    {
        var patient = await _context.Patients
            .Include(p => p.Profile)
            .FirstOrDefaultAsync(p => p.Id == patientId)
            ?? throw new KeyNotFoundException("Patient not found");

        var profile = patient.Profile
            ?? throw new KeyNotFoundException("No profile exists for this patient yet. Create one first.");

        ValidateProfile(request);

        if (request.MedicalRecordNumber != null)
            patient.MedicalRecordNumber = request.MedicalRecordNumber;

        ApplyProfile(profile, request);

        await _context.SaveChangesAsync();

        return ToProfileResponse(profile);
    }

    public async Task<PatientHistoryResponse?> GetHistoryAsync(Guid patientId)
    {
        var patient = await BaseQuery().FirstOrDefaultAsync(p => p.Id == patientId);
        if (patient == null)
            return null;

        var appointments = await _context.Appointments
            .Include(a => a.Status)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.ScheduledStart)
            .ToListAsync();

        var appointmentResponses = appointments.Select(a => new Application.Features.Appointments.AppointmentResponse
        {
            Id = a.Id,
            PatientId = a.PatientId,
            PatientName = $"{a.Patient?.User?.FirstName} {a.Patient?.User?.LastName}",
            DoctorId = a.DoctorId,
            DoctorName = $"{a.Doctor?.User?.FirstName} {a.Doctor?.User?.LastName}",
            Status = a.Status?.Name ?? string.Empty,
            ScheduledStart = a.ScheduledStart,
            ScheduledEnd = a.ScheduledEnd,
            ReasonForVisit = a.ReasonForVisit,
            Notes = a.Notes,
            CancellationReason = a.CancellationReason,
            CancelledAt = a.CancelledAt,
            CreatedAt = a.CreatedAt
        }).ToList();

        return new PatientHistoryResponse
        {
            PatientId = patient.Id,
            FullName = $"{patient.User.FirstName} {patient.User.LastName}",
            Email = patient.User.Email,
            Appointments = appointmentResponses,
            TotalAppointments = appointments.Count,
            CompletedAppointments = appointments.Count(a => a.Status?.Name == "Completed"),
            CancelledAppointments = appointments.Count(a => a.Status?.Name == "Cancelled")
        };
    }

    private static void ValidateProfile(PatientProfileRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.EmergencyContactPhone) &&
            !ValidationHelpers.IsValidPhone(request.EmergencyContactPhone))
        {
            throw new ArgumentException("Emergency contact phone number is not valid");
        }

        if (request.DateOfBirth.HasValue && request.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Date of birth cannot be in the future");
        }
    }

    private static void ApplyProfile(PatientProfile profile, PatientProfileRequest request)
    {
        profile.DateOfBirth = request.DateOfBirth;
        profile.Gender = request.Gender;
        profile.BloodGroup = request.BloodGroup;
        profile.HeightCm = request.HeightCm;
        profile.WeightKg = request.WeightKg;
        profile.AddressLine1 = request.AddressLine1;
        profile.AddressLine2 = request.AddressLine2;
        profile.City = request.City;
        profile.State = request.State;
        profile.PostalCode = request.PostalCode;
        profile.Country = request.Country;
        profile.EmergencyContactName = request.EmergencyContactName;
        profile.EmergencyContactPhone = request.EmergencyContactPhone;
        profile.Allergies = request.Allergies;
        profile.ChronicConditions = request.ChronicConditions;
        profile.CurrentMedications = request.CurrentMedications;
    }

    private IQueryable<Patient> BaseQuery() =>
        _context.Patients
            .Include(p => p.User)
            .Include(p => p.Profile)
            .AsQueryable();

    private static PatientResponse ToResponse(Patient patient) => new()
    {
        Id = patient.Id,
        UserId = patient.UserId,
        FullName = $"{patient.User.FirstName} {patient.User.LastName}",
        Email = patient.User.Email,
        PhoneNumber = patient.User.PhoneNumber,
        MedicalRecordNumber = patient.MedicalRecordNumber,
        Profile = patient.Profile == null ? null : ToProfileResponse(patient.Profile),
        CreatedAt = patient.CreatedAt
    };

    private static PatientProfileResponse ToProfileResponse(PatientProfile profile) => new()
    {
        DateOfBirth = profile.DateOfBirth,
        Gender = profile.Gender,
        BloodGroup = profile.BloodGroup,
        HeightCm = profile.HeightCm,
        WeightKg = profile.WeightKg,
        AddressLine1 = profile.AddressLine1,
        AddressLine2 = profile.AddressLine2,
        City = profile.City,
        State = profile.State,
        PostalCode = profile.PostalCode,
        Country = profile.Country,
        EmergencyContactName = profile.EmergencyContactName,
        EmergencyContactPhone = profile.EmergencyContactPhone,
        Allergies = profile.Allergies,
        ChronicConditions = profile.ChronicConditions,
        CurrentMedications = profile.CurrentMedications
    };
}
