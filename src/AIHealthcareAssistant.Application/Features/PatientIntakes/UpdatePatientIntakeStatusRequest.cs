using System.ComponentModel.DataAnnotations;
using AIHealthcareAssistant.Domain.Enums;

namespace AIHealthcareAssistant.Application.Features.PatientIntakes;

public class UpdatePatientIntakeStatusRequest
{
    [EnumDataType(typeof(IntakeStatus), ErrorMessage = "Status must be a valid intake status.")]
    public IntakeStatus Status { get; set; }
}
