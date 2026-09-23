using System.Data;
using AIHealthcareAssistant.Application.Features.Appointments;
using AIHealthcareAssistant.Domain.Entities;
using AIHealthcareAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIHealthcareAssistant.Infrastructure.Services;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _context;

    public AppointmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request)
    {
        var patient = await _context.Patients.FindAsync(request.PatientId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found");

        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId);
        if (doctor == null)
            throw new KeyNotFoundException("Doctor not found");

        if (request.ScheduledStart <= DateTime.UtcNow)
            throw new ArgumentException("Cannot book appointment in the past");

        if (request.ScheduledEnd <= request.ScheduledStart)
            throw new ArgumentException("End time must be after start time");

        var pendingStatus = await _context.AppointmentStatuses
            .FirstOrDefaultAsync(s => s.Name == "Pending");
        if (pendingStatus == null)
            throw new InvalidOperationException("Appointment status not configured");

        var cancelledStatus = await _context.AppointmentStatuses
            .FirstOrDefaultAsync(s => s.Name == "Cancelled");

        var cancelledStatusId = cancelledStatus?.Id ?? Guid.Empty;

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var hasConflict = await _context.Appointments
            .AnyAsync(a => a.DoctorId == request.DoctorId
                && a.AppointmentStatusId != cancelledStatusId
                && a.ScheduledStart < request.ScheduledEnd
                && a.ScheduledEnd > request.ScheduledStart);

        if (hasConflict)
            throw new InvalidOperationException("Doctor is not available at this time slot");

        var hasAvailability = await _context.DoctorAvailabilities
            .AnyAsync(a => a.DoctorId == request.DoctorId
                && a.DayOfWeek == request.ScheduledStart.DayOfWeek
                && a.IsActive
                && a.StartTime <= TimeOnly.FromDateTime(request.ScheduledStart)
                && a.EndTime >= TimeOnly.FromDateTime(request.ScheduledEnd)
                && (a.EffectiveFrom == null || a.EffectiveFrom <= DateOnly.FromDateTime(request.ScheduledStart))
                && (a.EffectiveTo == null || a.EffectiveTo >= DateOnly.FromDateTime(request.ScheduledStart)));

        if (!hasAvailability)
            throw new InvalidOperationException("Doctor has no availability at this time slot");

        var patientHasConflict = await _context.Appointments
            .AnyAsync(a => a.PatientId == request.PatientId
                && a.AppointmentStatusId != cancelledStatusId
                && a.ScheduledStart < request.ScheduledEnd
                && a.ScheduledEnd > request.ScheduledStart);

        if (patientHasConflict)
            throw new InvalidOperationException("Patient already has an appointment at this time");

        PatientIntake? linkedIntake;
        if (request.PatientIntakeId.HasValue)
        {
            linkedIntake = await _context.PatientIntakes
                .FirstOrDefaultAsync(i => i.Id == request.PatientIntakeId.Value);

            if (linkedIntake == null)
                throw new KeyNotFoundException("Patient intake was not found");

            if (linkedIntake.PatientId != request.PatientId)
                throw new ArgumentException("Patient intake does not belong to this patient");

            if (linkedIntake.AppointmentId != null)
                throw new InvalidOperationException("Patient intake is already linked to another appointment");
        }
        else
        {
            linkedIntake = await _context.PatientIntakes
                .Where(i => i.PatientId == request.PatientId && i.AppointmentId == null)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();
        }

        AIConversation? linkedConversation;
        if (request.AIConversationId.HasValue)
        {
            linkedConversation = await _context.AIConversations
                .FirstOrDefaultAsync(c => c.Id == request.AIConversationId.Value);

            if (linkedConversation == null)
                throw new KeyNotFoundException("AI conversation was not found");

            if (linkedConversation.PatientId != request.PatientId)
                throw new ArgumentException("AI conversation does not belong to this patient");

            if (linkedConversation.AppointmentId != null)
                throw new InvalidOperationException("AI conversation is already linked to another appointment");
        }
        else
        {
            linkedConversation = await _context.AIConversations
                .Where(c => c.PatientId == request.PatientId && c.AppointmentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            AppointmentStatusId = pendingStatus.Id,
            Status = pendingStatus,
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledEnd,
            ReasonForVisit = request.ReasonForVisit,
            Notes = request.Notes,
            PatientIntakeId = linkedIntake?.Id,
            AIConversationId = linkedConversation?.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);

        if (linkedIntake != null)
            linkedIntake.AppointmentId = appointment.Id;

        if (linkedConversation != null)
            linkedConversation.AppointmentId = appointment.Id;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        var user = await _context.Users.FindAsync(patient.UserId);

        return new AppointmentResponse
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = user != null ? $"{user.FirstName} {user.LastName}" : string.Empty,
            DoctorId = appointment.DoctorId,
            DoctorName = $"{doctor.User.FirstName} {doctor.User.LastName}",
            Status = pendingStatus.Name,
            ScheduledStart = appointment.ScheduledStart,
            ScheduledEnd = appointment.ScheduledEnd,
            ReasonForVisit = appointment.ReasonForVisit,
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt
        };
    }

    public async Task<AppointmentResponse?> GetByIdAsync(Guid id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Status)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Intake)
            .Include(a => a.Conversation)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            return null;

        return MapToResponse(appointment);
    }

    public async Task<List<AppointmentResponse>> GetByPatientAsync(Guid patientId)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Status)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Intake)
            .Include(a => a.Conversation)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.ScheduledStart)
            .ToListAsync();

        return appointments.Select(MapToResponse).ToList();
    }

    public async Task<List<AppointmentResponse>> GetByDoctorAsync(Guid doctorId)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Status)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Intake)
            .Include(a => a.Conversation)
            .Where(a => a.DoctorId == doctorId)
            .OrderByDescending(a => a.ScheduledStart)
            .ToListAsync();

        return appointments.Select(MapToResponse).ToList();
    }

    public async Task<AppointmentResponse> CancelAsync(Guid id, string? cancellationReason)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Status)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Intake)
            .Include(a => a.Conversation)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            throw new KeyNotFoundException("Appointment not found");

        if (appointment.Status?.Name == "Cancelled")
            throw new InvalidOperationException("Appointment is already cancelled");

        if (appointment.Status?.Name == "Completed")
            throw new InvalidOperationException("Cannot cancel a completed appointment");

        var cancelledStatus = await _context.AppointmentStatuses
            .FirstOrDefaultAsync(s => s.Name == "Cancelled");
        if (cancelledStatus == null)
            throw new InvalidOperationException("Cancelled status not configured");

        appointment.AppointmentStatusId = cancelledStatus.Id;
        appointment.Status = cancelledStatus;
        appointment.CancellationReason = cancellationReason;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponse> RescheduleAsync(Guid id, RescheduleAppointmentRequest request)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Status)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Intake)
            .Include(a => a.Conversation)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            throw new KeyNotFoundException("Appointment not found");

        if (appointment.Status?.Name == "Cancelled")
            throw new InvalidOperationException("Cannot reschedule a cancelled appointment");

        if (appointment.Status?.Name == "Completed")
            throw new InvalidOperationException("Cannot reschedule a completed appointment");

        if (request.NewScheduledStart <= DateTime.UtcNow)
            throw new ArgumentException("Cannot reschedule to a past date");

        if (request.NewScheduledEnd <= request.NewScheduledStart)
            throw new ArgumentException("End time must be after start time");

        var cancelledStatus = await _context.AppointmentStatuses
            .FirstOrDefaultAsync(s => s.Name == "Cancelled");
        var cancelledStatusId = cancelledStatus?.Id ?? Guid.Empty;

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        var hasConflict = await _context.Appointments
            .AnyAsync(a => a.DoctorId == appointment.DoctorId
                && a.Id != id
                && a.AppointmentStatusId != cancelledStatusId
                && a.ScheduledStart < request.NewScheduledEnd
                && a.ScheduledEnd > request.NewScheduledStart);

        if (hasConflict)
            throw new InvalidOperationException("Doctor is not available at the new time slot");

        var hasAvailability = await _context.DoctorAvailabilities
            .AnyAsync(a => a.DoctorId == appointment.DoctorId
                && a.DayOfWeek == request.NewScheduledStart.DayOfWeek
                && a.IsActive
                && a.StartTime <= TimeOnly.FromDateTime(request.NewScheduledStart)
                && a.EndTime >= TimeOnly.FromDateTime(request.NewScheduledEnd)
                && (a.EffectiveFrom == null || a.EffectiveFrom <= DateOnly.FromDateTime(request.NewScheduledStart))
                && (a.EffectiveTo == null || a.EffectiveTo >= DateOnly.FromDateTime(request.NewScheduledStart)));

        if (!hasAvailability)
            throw new InvalidOperationException("Doctor has no active availability schedule at the new time slot");

        var patientHasConflict = await _context.Appointments
            .AnyAsync(a => a.PatientId == appointment.PatientId
                && a.Id != id
                && a.AppointmentStatusId != cancelledStatusId
                && a.ScheduledStart < request.NewScheduledEnd
                && a.ScheduledEnd > request.NewScheduledStart);

        if (patientHasConflict)
            throw new InvalidOperationException("Patient already has another appointment at this time");

        appointment.ScheduledStart = request.NewScheduledStart;
        appointment.ScheduledEnd = request.NewScheduledEnd;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponse> UpdateStatusAsync(Guid id, string status)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Status)
            .Include(a => a.Patient)
            .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Include(a => a.Intake)
            .Include(a => a.Conversation)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            throw new KeyNotFoundException("Appointment not found");

        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required.");

        var newStatus = await _context.AppointmentStatuses
            .FirstOrDefaultAsync(s => s.Name == status);
        if (newStatus == null)
            throw new ArgumentException($"Status '{status}' not found");

        var currentName = appointment.Status?.Name;

        if (!string.Equals(currentName, newStatus.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(currentName, "Cancelled", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "Cannot change the status of a cancelled appointment. Please book a new appointment.");

            if (string.Equals(currentName, "Completed", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "Cannot change the status of a completed appointment.");
        }

        appointment.AppointmentStatusId = newStatus.Id;
        appointment.Status = newStatus;

        if (string.Equals(newStatus.Name, "Cancelled", StringComparison.OrdinalIgnoreCase)
            && appointment.CancelledAt == null)
        {
            appointment.CancelledAt = DateTime.UtcNow;
        }

        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponse(appointment);
    }

    public async Task<List<AppointmentResponse>> GetDoctorDashboardAppointmentsAsync(
        Guid doctorId,
        string? status = null,
        DateTime? date = null)
    {
        var query = _context.Appointments
            .Include(a => a.Status)
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Intake)
            .Include(a => a.Conversation)
            .Where(a => a.DoctorId == doctorId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(a => a.Status != null && a.Status.Name == status);
        }

        if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            query = query.Where(a => a.ScheduledStart.Date == targetDate);
        }

        var appointments = await query
            .OrderBy(a => a.ScheduledStart)
            .ToListAsync();

        return appointments.Select(MapToResponse).ToList();
    }

    private static AppointmentResponse MapToResponse(Appointment appointment)
    {
        var patientUser = appointment.Patient?.User;
        var doctorUser = appointment.Doctor?.User;

        return new AppointmentResponse
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = patientUser != null ? $"{patientUser.FirstName} {patientUser.LastName}" : string.Empty,
            DoctorId = appointment.DoctorId,
            DoctorName = doctorUser != null ? $"{doctorUser.FirstName} {doctorUser.LastName}" : string.Empty,
            Status = appointment.Status?.Name ?? string.Empty,
            ScheduledStart = appointment.ScheduledStart,
            ScheduledEnd = appointment.ScheduledEnd,
            ReasonForVisit = appointment.ReasonForVisit,
            Notes = appointment.Notes,
            CancellationReason = appointment.CancellationReason,
            CancelledAt = appointment.CancelledAt,
            CreatedAt = appointment.CreatedAt,

            PatientIntakeId = appointment.Intake?.Id ?? appointment.PatientIntakeId,
            ChiefComplaint = appointment.Intake?.ChiefComplaint,
            Symptoms = appointment.Intake?.Symptoms,

            AIConversationId = appointment.Conversation?.Id ?? appointment.AIConversationId,
            AISummary = appointment.Conversation?.Summary
        };
    }
}
