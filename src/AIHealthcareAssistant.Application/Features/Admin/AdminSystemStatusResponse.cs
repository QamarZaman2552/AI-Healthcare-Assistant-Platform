using System;
using System.Collections.Generic;
using System.Text;

namespace AIHealthcareAssistant.Application.Features.Admin;

public class AdminSystemStatusResponse
{
    public string Status { get; set; } = string.Empty;

    public bool DatabaseConnected { get; set; }

    public bool AiServiceHealthy { get; set; }

    public DateTime CheckedAtUtc { get; set; } 

}
