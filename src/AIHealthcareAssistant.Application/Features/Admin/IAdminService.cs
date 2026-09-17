using System;
using System.Collections.Generic;
using System.Text;

namespace AIHealthcareAssistant.Application.Features.Admin;

public interface IAdminService
{
    Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default);

    Task<List<AdminUserResponse>> GetUsersAsync(CancellationToken cancellation = default);

    Task<AdminSystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellation = default);
}
