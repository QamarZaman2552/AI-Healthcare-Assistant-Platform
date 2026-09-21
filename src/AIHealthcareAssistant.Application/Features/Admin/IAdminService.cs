<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Text;
=======
using AIHealthcareAssistant.Application.Common.Models;
>>>>>>> origin/develop

namespace AIHealthcareAssistant.Application.Features.Admin;

public interface IAdminService
{
<<<<<<< HEAD
    Task<AdminDashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default);

    Task<List<AdminUserResponse>> GetUsersAsync(CancellationToken cancellation = default);

    Task<AdminSystemStatusResponse> GetSystemStatusAsync(CancellationToken cancellation = default);
=======
    Task<AdminDashboardResponse> GetDashboardAsync();
    Task<List<UserResponse>> GetUsersAsync();
    Task<SystemStatusResponse> GetSystemStatusAsync();
>>>>>>> origin/develop
}
