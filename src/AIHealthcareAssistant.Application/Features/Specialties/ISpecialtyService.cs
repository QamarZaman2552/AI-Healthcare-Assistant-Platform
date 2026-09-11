namespace AIHealthcareAssistant.Application.Features.Specialties;

public interface ISpecialtyService
{
    Task<SpecialtyResponse> CreateAsync(CreateSpecialtyRequest request);
    Task<SpecialtyResponse> UpdateAsync(Guid id, UpdateSpecialtyRequest request);
    Task<SpecialtyResponse?> GetByIdAsync(Guid id);
    Task<List<SpecialtyResponse>> GetAllAsync();
}
