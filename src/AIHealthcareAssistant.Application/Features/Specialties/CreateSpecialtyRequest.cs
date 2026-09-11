namespace AIHealthcareAssistant.Application.Features.Specialties;

public class CreateSpecialtyRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
