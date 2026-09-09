namespace AIHealthcareAssistant.Application.Features.Availability;

public interface IAvailabilityService
{
    Task<AvailabilityResponse> CreateAsync(CreateAvailabilityRequest request);
    Task<AvailabilityResponse> UpdateAsync(Guid id, UpdateAvailabilityRequest request);
    Task<AvailabilityResponse?> GetByIdAsync(Guid id);
    Task<List<AvailabilityResponse>> GetByDoctorAsync(Guid doctorId);
    Task<List<AvailabilityResponse>> GetByDoctorAndDayAsync(Guid doctorId, DayOfWeek dayOfWeek);
    Task<List<TimeSlotResponse>> GetAvailableSlotsAsync(Guid doctorId, DateOnly date);
    Task DeleteAsync(Guid id);
}