namespace AIHealthcareAssistant.Application.Features.Admin;

public class SystemStatusResponse
{
    public string Status { get; set; } = string.Empty;
    public bool DatabaseConnected { get; set; }
    public bool AIAvailable { get; set; }
    public int ActiveConnections { get; set; }
    public DateTime LastChecked { get; set; }
}
