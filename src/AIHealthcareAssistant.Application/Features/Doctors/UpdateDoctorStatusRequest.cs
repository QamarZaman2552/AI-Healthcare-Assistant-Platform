using System.ComponentModel.DataAnnotations;

namespace AIHealthcareAssistant.Application.Features.Doctors;

public class UpdateDoctorStatusRequest
{
    [Required(ErrorMessage = "IsActive is required.")]
    public bool? IsActive { get; set; }
}
