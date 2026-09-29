namespace AIHealthcareAssistant.Application.Features.Ai
{
    public class AIChatResponseDto
    {
        public Guid ConversationId { get; set; }

        public string Message { get; set; } = string.Empty;


    }
}
