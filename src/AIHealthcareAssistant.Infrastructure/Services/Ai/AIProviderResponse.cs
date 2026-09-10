using System;
using System.Collections.Generic;
using System.Text;

namespace AIHealthcareAssistant.Infrastructure.Services.Ai;

internal class AIProviderResponse
{
    public List<AIProviderChoice> Choices { get; set; } = new();
}
