using System.ComponentModel.DataAnnotations;
namespace AIHealthcareAssistant.Application.Features.Specialties;

public class CreateSpecialtyRequest
{
    [Required(ErrorMessage = "Specialty name is required.")]
    [MinLength(2)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
