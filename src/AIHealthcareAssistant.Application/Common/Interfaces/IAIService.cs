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

    Task<List<ConversationResponse>> GetConversationsByPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken = default);

    Task<ConversationDetailResponse> GetConversationByIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<ConversationResponse> CreateConversationAsync(
        CreateConversationRequest request,
        CancellationToken cancellationToken = default);
}