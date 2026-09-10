using System;
using System.Collections.Generic;
using System.Text;

namespace AIHealthcareAssistant.Infrastructure.Services.Ai;

internal class AIProviderRequest
{
    public string Model { get; set; } = string.Empty;

    public List<AIProviderMessage> Messages { get; set; } = new();

    public double Temperature { get; set; } = 0.2;
}
