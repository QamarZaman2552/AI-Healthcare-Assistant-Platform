namespace AIHealthcareAssistant.Application.Features.Appointments;

public interface IAppointmentService
{
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request);
    Task<AppointmentResponse?> GetByIdAsync(Guid id);
    Task<List<AppointmentResponse>> GetByPatientAsync(Guid patientId);
    Task<List<AppointmentResponse>> GetByDoctorAsync(Guid doctorId);
    Task<AppointmentResponse> CancelAsync(Guid id, string? cancellationReason);
    Task<AppointmentResponse> RescheduleAsync(Guid id, RescheduleAppointmentRequest request);
    Task<AppointmentResponse> UpdateStatusAsync(Guid id, string status);

    Task<List<AppointmentResponse>> GetDoctorDashboardAppointmentsAsync( Guid doctorId,string? status = null,DateTime? date = null);
}