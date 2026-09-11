namespace AIHealthcareAssistant.Application.Features.Doctors;

public class AssignSpecialtyRequest
{
    public Guid SpecialtyId { get; set; }
    public bool IsPrimary { get; set; }
}
