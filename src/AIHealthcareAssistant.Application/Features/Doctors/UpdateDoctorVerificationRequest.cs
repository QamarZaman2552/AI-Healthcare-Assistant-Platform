using System.ComponentModel.DataAnnotations;

namespace AIHealthcareAssistant.Application.Features.Doctors;

public class UpdateDoctorVerificationRequest
{
    [Required(ErrorMessage = "IsVerified is required.")]
    public bool? IsVerified { get; set; }
}
