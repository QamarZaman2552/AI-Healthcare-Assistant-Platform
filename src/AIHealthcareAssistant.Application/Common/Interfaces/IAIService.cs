using AIHealthcareAssistant.Application.Features.Ai;

namespace AIHealthcareAssistant.Application.Common.Interfaces;

public interface IAIService
{
    Task<AIChatResponseDto> ChatAsync(
        AIChatRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AISymptomCheckResponseDto> SymptomCheckAsync(
        AISymptomCheckRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> HealthCheckAsync(
        CancellationToken cancellationToken = default);
}