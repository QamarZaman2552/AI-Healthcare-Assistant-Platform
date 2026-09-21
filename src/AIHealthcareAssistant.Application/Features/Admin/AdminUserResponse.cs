using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AIHealthcareAssistant.Application.Features.Admin;

public class AdminUserResponse
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Roles { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
