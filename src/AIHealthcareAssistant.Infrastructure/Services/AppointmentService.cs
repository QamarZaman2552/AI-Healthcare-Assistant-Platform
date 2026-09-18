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
            throw new InvalidOperationException("Cannot book appointment in the past");

        if (request.ScheduledEnd <= request.ScheduledStart)
            throw new InvalidOperationException("End time must be after start time");

        var pendingStatus = await _context.AppointmentStatuses
            .FirstOrDefaultAsync(s => s.Name == "Pending");
        if (pendingStatus == null)
            throw new InvalidOperationException("Appointment status not configured");

        var cancelledStatus = await _context.AppointmentStatuses
            .FirstOrDefaultAsync(s => s.Name == "Cancelled");

        var cancelledStatusId = cancelledStatus?.Id ?? Guid.Empty;

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
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

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
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            throw new KeyNotFoundException("Appointment not found");

        if (appointment.Status?.Name == "Cancelled")
            throw new InvalidOperationException("Cannot reschedule a cancelled appointment");

        if (appointment.Status?.Name == "Completed")
            throw new InvalidOperationException("Cannot reschedule a completed appointment");

        if (request.NewScheduledStart <= DateTime.UtcNow)
            throw new InvalidOperationException("Cannot reschedule to a past date");

        if (request.NewScheduledEnd <= request.NewScheduledStart)
            throw new InvalidOperationException("End time must be after start time");

        var cancelledStatus = await _context.AppointmentStatuses
            .FirstOrDefaultAsync(s => s.Name == "Cancelled");
        var cancelledStatusId = cancelledStatus?.Id ?? Guid.Empty;

        var hasConflict = await _context.Appointments
            .AnyAsync(a => a.DoctorId == appointment.DoctorId
                && a.Id != id
                && a.AppointmentStatusId != cancelledStatusId
                && a.ScheduledStart < request.NewScheduledEnd
                && a.ScheduledEnd > request.NewScheduledStart);

        if (hasConflict)
            throw new InvalidOperationException("Doctor is not available at the new time slot");

        appointment.ScheduledStart = request.NewScheduledStart;
        appointment.ScheduledEnd = request.NewScheduledEnd;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

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
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            throw new KeyNotFoundException("Appointment not found");

        var newStatus = await _context.AppointmentStatuses
            .FirstOrDefaultAsync(s => s.Name == status);
        if (newStatus == null)
            throw new InvalidOperationException($"Status '{status}' not found");

        appointment.AppointmentStatusId = newStatus.Id;
        appointment.Status = newStatus;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponse(appointment);
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
            CreatedAt = appointment.CreatedAt
        };
    }
}