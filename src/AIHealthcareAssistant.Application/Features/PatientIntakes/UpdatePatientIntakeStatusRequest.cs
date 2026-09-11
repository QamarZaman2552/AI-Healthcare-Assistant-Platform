using AIHealthcareAssistant.Domain.Enums;

namespace AIHealthcareAssistant.Application.Features.PatientIntakes;

public class UpdatePatientIntakeStatusRequest
{
    public IntakeStatus Status { get; set; }
}
