using System.Text.RegularExpressions;

namespace AIHealthcareAssistant.Application.Common.Validation;

public static class ValidationHelpers
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private static readonly Regex PhoneRegex =
        new(@"^\+?[0-9\s\-()]{7,20}$", RegexOptions.Compiled);

    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);

    public static bool IsValidPhone(string? phone) =>
        !string.IsNullOrWhiteSpace(phone) && PhoneRegex.IsMatch(phone);
}
