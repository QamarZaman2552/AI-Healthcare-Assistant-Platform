using AIHealthcareAssistant.Application.Common.Models;
using AIHealthcareAssistant.Application.Features.Specialties;

namespace AIHealthcareAssistant.Application.Features.Doctors;

public interface IDoctorService
{
    Task<DoctorResponse> CreateAsync(CreateDoctorRequest request);
    Task<DoctorResponse> UpdateAsync(Guid id, UpdateDoctorRequest request);
    Task<DoctorResponse?> GetByIdAsync(Guid id);
    Task<DoctorResponse?> GetByUserIdAsync(Guid userId);
    Task<PagedResult<DoctorResponse>> GetDoctorsAsync(DoctorQuery query);
    Task<DoctorResponse> UpdateStatusAsync(Guid id, bool isActive);
    Task<DoctorResponse> UpdateVerificationAsync(Guid id, bool isVerified);

    Task<SpecialtyResponse> AssignSpecialtyAsync(Guid doctorId, AssignSpecialtyRequest request);
    Task RemoveSpecialtyAsync(Guid doctorId, Guid specialtyId);
    Task<List<SpecialtyResponse>> GetSpecialtiesByDoctorAsync(Guid doctorId);
    Task<List<DoctorResponse>> GetDoctorsBySpecialtyAsync(Guid specialtyId);
}
