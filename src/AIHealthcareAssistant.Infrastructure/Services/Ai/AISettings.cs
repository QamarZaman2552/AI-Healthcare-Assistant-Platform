using System;
using System.Collections.Generic;
using System.Text;

namespace AIHealthcareAssistant.Infrastructure.Services.Ai;

public class AISettings
{
    public string BaseUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 30;
}