using AIHealthcareAssistant.Application.Features.Availability;
using AIHealthcareAssistant.Domain.Entities;
using AIHealthcareAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIHealthcareAssistant.Infrastructure.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly AppDbContext _context;

    public AvailabilityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AvailabilityResponse> CreateAsync(CreateAvailabilityRequest request)
    {
        var doctor = await _context.Doctors
       .Include(d => d.User)
       .FirstOrDefaultAsync(d => d.Id == request.DoctorId);  

        if (doctor == null)
            throw new KeyNotFoundException("Doctor not found");

        if (request.EndTime <= request.StartTime)
            throw new InvalidOperationException("End time must be after start time");

        if (request.SlotDurationMinutes <= 0)
            throw new InvalidOperationException("Slot duration must be greater than 0");

        var existing = await _context.DoctorAvailabilities
            .FirstOrDefaultAsync(a => a.DoctorId == request.DoctorId
                && a.DayOfWeek == request.DayOfWeek
                && a.StartTime < request.EndTime
                && a.EndTime > request.StartTime);

        if (existing != null)
            throw new InvalidOperationException("Availability overlaps with existing schedule");

        var availability = new DoctorAvailability
        {
            Id = Guid.NewGuid(),
            DoctorId = request.DoctorId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SlotDurationMinutes = request.SlotDurationMinutes,
            IsActive = true,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo
        };

        _context.DoctorAvailabilities.Add(availability);
        await _context.SaveChangesAsync();

        return MapToResponse(availability, doctor);
    }

    public async Task<AvailabilityResponse> UpdateAsync(
     Guid id,
     UpdateAvailabilityRequest request)
    {
        var availability = await _context.DoctorAvailabilities
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (availability == null)
            throw new KeyNotFoundException("Availability not found");

        if (request.EndTime <= request.StartTime)
            throw new InvalidOperationException(
                "End time must be after start time");

        var existing = await _context.DoctorAvailabilities
            .FirstOrDefaultAsync(a => a.DoctorId == availability.DoctorId
                && a.Id != id
                && a.DayOfWeek == request.DayOfWeek
                && a.StartTime < request.EndTime
                && a.EndTime > request.StartTime);

        if (existing != null)
            throw new InvalidOperationException("Availability overlaps with existing schedule");

        availability.DayOfWeek = request.DayOfWeek;
        availability.StartTime = request.StartTime;
        availability.EndTime = request.EndTime;
        availability.SlotDurationMinutes = request.SlotDurationMinutes;
        availability.IsActive = request.IsActive;
        availability.EffectiveFrom = request.EffectiveFrom;
        availability.EffectiveTo = request.EffectiveTo;

        await _context.SaveChangesAsync();

        return MapToResponse(
            availability,
            availability.Doctor);
    }

    public async Task<AvailabilityResponse?> GetByIdAsync(Guid id)
    {
        var availability = await _context.DoctorAvailabilities
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (availability == null)
            return null;

        return MapToResponse(availability, availability.Doctor);
    }

    public async Task<List<AvailabilityResponse>> GetByDoctorAsync(Guid doctorId)
    {
        var availabilities = await _context.DoctorAvailabilities
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Where(a => a.DoctorId == doctorId && a.IsActive)
            .OrderBy(a => a.DayOfWeek)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        return availabilities.Select(a => MapToResponse(a, a.Doctor)).ToList();
    }

    public async Task<List<AvailabilityResponse>> GetByDoctorAndDayAsync(Guid doctorId, DayOfWeek dayOfWeek)
    {
        var availabilities = await _context.DoctorAvailabilities
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Where(a => a.DoctorId == doctorId
                && a.DayOfWeek == dayOfWeek
                && a.IsActive)
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        return availabilities.Select(a => MapToResponse(a, a.Doctor)).ToList();
    }

    public async Task<List<TimeSlotResponse>> GetAvailableSlotsAsync(Guid doctorId, DateOnly date)
    {
        var dayOfWeek = date.DayOfWeek;

        var availabilities = await _context.DoctorAvailabilities
            .Where(a => a.DoctorId == doctorId
                && a.DayOfWeek == dayOfWeek
                && a.IsActive
                && (a.EffectiveFrom == null || a.EffectiveFrom <= date)
                && (a.EffectiveTo == null || a.EffectiveTo >= date))
            .ToListAsync();

        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var cancelledStatus = await _context.AppointmentStatuses
            .FirstOrDefaultAsync(s => s.Name == "Cancelled");

        var cancelledStatusId = cancelledStatus?.Id ?? Guid.Empty;

        var bookedAppointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId
                && a.ScheduledStart >= startOfDay
                && a.ScheduledStart < endOfDay
                && a.AppointmentStatusId != cancelledStatusId)
            .ToListAsync();

        var slots = new List<TimeSlotResponse>();

        foreach (var availability in availabilities)
        {
            var current = availability.StartTime;
            while (current.AddMinutes(availability.SlotDurationMinutes) <= availability.EndTime)
            {
                var slotEnd = current.AddMinutes(availability.SlotDurationMinutes);

                var isBooked = bookedAppointments.Any(a =>
                    TimeOnly.FromDateTime(a.ScheduledStart) < slotEnd &&
                    TimeOnly.FromDateTime(a.ScheduledEnd) > current);

                slots.Add(new TimeSlotResponse
                {
                    StartTime = current,
                    EndTime = slotEnd,
                    IsAvailable = !isBooked
                });

                current = slotEnd;
            }
        }

        return slots.OrderBy(s => s.StartTime).ToList();
    }

    public async Task DeleteAsync(Guid id)
    {
        var availability = await _context.DoctorAvailabilities.FindAsync(id);
        if (availability == null)
            throw new KeyNotFoundException("Availability not found");

        _context.DoctorAvailabilities.Remove(availability);
        await _context.SaveChangesAsync();
    }

    private static AvailabilityResponse MapToResponse(DoctorAvailability availability, Doctor doctor)
    {
        return new AvailabilityResponse
        {
            Id = availability.Id,
            DoctorId = availability.DoctorId,
            DoctorName = $"{doctor.User.FirstName} {doctor.User.LastName}",
            DayOfWeek = availability.DayOfWeek,
            StartTime = availability.StartTime,
            EndTime = availability.EndTime,
            SlotDurationMinutes = availability.SlotDurationMinutes,
            IsActive = availability.IsActive,
            EffectiveFrom = availability.EffectiveFrom,
            EffectiveTo = availability.EffectiveTo
        };
    }
}