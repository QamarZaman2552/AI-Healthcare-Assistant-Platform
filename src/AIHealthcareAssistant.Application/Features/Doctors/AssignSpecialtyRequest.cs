using AIHealthcareAssistant.Application.Common.Validation;

namespace AIHealthcareAssistant.Application.Features.Doctors;

public class AssignSpecialtyRequest
{
    [NotEmptyGuid(ErrorMessage = "SpecialtyId is required.")]
    public Guid SpecialtyId { get; set; }
    public bool IsPrimary { get; set; }
}
