namespace AIHealthcareAssistant.Application.Features.Specialties;

public class SpecialtyResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
