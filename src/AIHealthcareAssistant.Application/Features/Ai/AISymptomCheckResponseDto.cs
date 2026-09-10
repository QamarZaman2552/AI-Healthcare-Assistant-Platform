namespace AIHealthcareAssistant.Application.Features.Ai;

public class AISymptomCheckResponseDto
{
    public string Summary { get; set; } = string.Empty;

    public string PossibleConditions { get; set; } = string.Empty;

    public string RecommendedAction { get; set; } = string.Empty;

    public string Urgency { get; set; } = string.Empty;

    public bool RequiresEmergencyCare { get; set; }
}