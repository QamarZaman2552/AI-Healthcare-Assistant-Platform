namespace AIHealthcareAssistant.Application.Features.Patients;

public interface IPatientService
{
    Task<PatientResponse?> GetByIdAsync(Guid id);
    Task<PatientResponse?> GetByUserIdAsync(Guid userId);
    Task<List<PatientResponse>> GetAllAsync();
    Task<PatientProfileResponse?> GetProfileAsync(Guid patientId);
    Task<PatientProfileResponse> CreateProfileAsync(Guid patientId, PatientProfileRequest request);
    Task<PatientProfileResponse> UpdateProfileAsync(Guid patientId, PatientProfileRequest request);
    Task<PatientHistoryResponse?> GetHistoryAsync(Guid patientId);
    Task<PatientResponse> RegisterAsync(Guid userId);
    Task<PatientDashboardResponse> GetDashboardStatsAsync(Guid patientId);
}
