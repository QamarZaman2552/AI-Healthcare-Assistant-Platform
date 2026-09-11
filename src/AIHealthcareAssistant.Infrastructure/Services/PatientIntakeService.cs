using AIHealthcareAssistant.Application.Features.PatientIntakes;
using AIHealthcareAssistant.Domain.Entities;
using AIHealthcareAssistant.Domain.Enums;
using AIHealthcareAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIHealthcareAssistant.Infrastructure.Services;

public class PatientIntakeService : IPatientIntakeService
{
    private readonly AppDbContext _context;

    public PatientIntakeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PatientIntakeResponse> CreateAsync(CreatePatientIntakeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ChiefComplaint))
            throw new ArgumentException("Chief complaint is required");

        if (!await _context.Patients.AnyAsync(p => p.Id == request.PatientId))
            throw new KeyNotFoundException("Patient not found");

        if (request.AppointmentId.HasValue &&
            !await _context.Appointments.AnyAsync(a => a.Id == request.AppointmentId.Value))
            throw new KeyNotFoundException("Appointment not found");

        if (request.AIConversationId.HasValue &&
            !await _context.AIConversations.AnyAsync(c => c.Id == request.AIConversationId.Value))
            throw new KeyNotFoundException("AI conversation not found");

        if (request.RecommendedSpecialtyId.HasValue &&
            !await _context.Specialties.AnyAsync(s => s.Id == request.RecommendedSpecialtyId.Value))
            throw new KeyNotFoundException("Recommended specialty not found");

        if (request.AppointmentId.HasValue &&
            await _context.PatientIntakes.AnyAsync(i => i.AppointmentId == request.AppointmentId.Value))
            throw new InvalidOperationException("An intake already exists for this appointment");

        var intake = new PatientIntake
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            AppointmentId = request.AppointmentId,
            AIConversationId = request.AIConversationId,
            RecommendedSpecialtyId = request.RecommendedSpecialtyId,
            ChiefComplaint = request.ChiefComplaint.Trim(),
            SymptomsDescription = request.SymptomsDescription,
            SymptomOnset = request.SymptomOnset,
            PainLevel = request.PainLevel,
            TemperatureCelsius = request.TemperatureCelsius,
            BloodPressure = request.BloodPressure,
            HeartRateBpm = request.HeartRateBpm,
            CurrentMedications = request.CurrentMedications,
            AdditionalNotes = request.AdditionalNotes,
            AISummary = request.AISummary,
            Status = string.IsNullOrWhiteSpace(request.AISummary) ? IntakeStatus.Submitted : IntakeStatus.AIProcessed,
            SubmittedAt = DateTime.UtcNow
        };

        _context.PatientIntakes.Add(intake);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(intake.Id) ?? throw new InvalidOperationException("Failed to load created intake");
    }

    public async Task<PatientIntakeResponse?> GetByIdAsync(Guid id)
    {
        var intake = await BaseQuery().FirstOrDefaultAsync(i => i.Id == id);
        return intake == null ? null : ToResponse(intake);
    }

    public async Task<List<PatientIntakeResponse>> GetByPatientAsync(Guid patientId)
    {
        var intakes = await BaseQuery()
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.SubmittedAt)
            .ToListAsync();

        return intakes.Select(ToResponse).ToList();
    }

    public async Task<PatientIntakeResponse?> GetByAppointmentAsync(Guid appointmentId)
    {
        var intake = await BaseQuery().FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);
        return intake == null ? null : ToResponse(intake);
    }

    public async Task<PatientIntakeResponse> UpdateStatusAsync(Guid id, IntakeStatus status)
    {
        var intake = await _context.PatientIntakes.FindAsync(id)
            ?? throw new KeyNotFoundException("Patient intake not found");

        intake.Status = status;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id) ?? throw new InvalidOperationException("Failed to load updated intake");
    }

    private IQueryable<PatientIntake> BaseQuery() =>
        _context.PatientIntakes
            .Include(i => i.Patient).ThenInclude(p => p.User)
            .Include(i => i.RecommendedSpecialty)
            .AsQueryable();

    private static PatientIntakeResponse ToResponse(PatientIntake intake) => new()
    {
        Id = intake.Id,
        PatientId = intake.PatientId,
        PatientName = $"{intake.Patient.User.FirstName} {intake.Patient.User.LastName}",
        AppointmentId = intake.AppointmentId,
        AIConversationId = intake.AIConversationId,
        RecommendedSpecialtyId = intake.RecommendedSpecialtyId,
        RecommendedSpecialtyName = intake.RecommendedSpecialty?.Name,
        ChiefComplaint = intake.ChiefComplaint,
        SymptomsDescription = intake.SymptomsDescription,
        SymptomOnset = intake.SymptomOnset,
        PainLevel = intake.PainLevel,
        TemperatureCelsius = intake.TemperatureCelsius,
        BloodPressure = intake.BloodPressure,
        HeartRateBpm = intake.HeartRateBpm,
        CurrentMedications = intake.CurrentMedications,
        AdditionalNotes = intake.AdditionalNotes,
        AISummary = intake.AISummary,
        Status = intake.Status,
        SubmittedAt = intake.SubmittedAt,
        CreatedAt = intake.CreatedAt
    };
}
