namespace AIHealthcareAssistant.Application.Features.PatientIntakes;

public interface IPatientIntakeService
{
    Task<PatientIntakeResponse> CreateAsync(CreatePatientIntakeRequest request);
    Task<PatientIntakeResponse?> GetByIdAsync(Guid id);
    Task<List<PatientIntakeResponse>> GetByPatientAsync(Guid patientId);
    Task<PatientIntakeResponse?> GetByAppointmentAsync(Guid appointmentId);
    Task<PatientIntakeResponse> UpdateStatusAsync(Guid id, Domain.Enums.IntakeStatus status);
}
