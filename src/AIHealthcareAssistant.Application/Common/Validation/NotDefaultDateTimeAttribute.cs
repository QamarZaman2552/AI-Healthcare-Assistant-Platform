using System.ComponentModel.DataAnnotations;

namespace AIHealthcareAssistant.Application.Common.Validation;

public sealed class NotDefaultDateTimeAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is DateTime dateTime && dateTime != default;
    }

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        if (value is DateTime dateTime && dateTime != default)
            return ValidationResult.Success;

        return new ValidationResult(
            ErrorMessage ?? $"{validationContext.DisplayName} is required.");
    }
}
