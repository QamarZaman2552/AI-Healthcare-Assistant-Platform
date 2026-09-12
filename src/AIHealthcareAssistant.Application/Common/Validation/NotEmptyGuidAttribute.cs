
using System.ComponentModel.DataAnnotations;

namespace AIHealthcareAssistant.Application.Common.Validation;

public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is Guid guid && guid != Guid.Empty;
    }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is Guid guid && guid != Guid.Empty)
            return ValidationResult.Success;

        return new ValidationResult(
            ErrorMessage ?? $"{validationContext.DisplayName} is required.");
    }
}

