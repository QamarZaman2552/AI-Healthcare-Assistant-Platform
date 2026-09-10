using System;
using System.Collections.Generic;
using System.Text;

namespace AIHealthcareAssistant.Infrastructure.Services.Ai;

internal class AIProviderMessage
{
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}