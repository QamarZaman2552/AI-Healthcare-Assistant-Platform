<<<<<<< HEAD
﻿using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Admin;
=======
﻿using AIHealthcareAssistant.Application.Features.Admin;
using AIHealthcareAssistant.Application.Common.Response;
>>>>>>> origin/develop
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

/// <summary>
/// Provides administrative dashboard, user management and system monitoring endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
<<<<<<< HEAD

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Gets real-time dashboard statistics for the administrator.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(
        typeof(ApiResponse<AdminDashboardResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AdminDashboardResponse>>> GetDashboard(
        CancellationToken cancellationToken)
    {
        var result = await _adminService.GetDashboardAsync(
            cancellationToken);

        return Ok(
            ApiResponse<AdminDashboardResponse>.Ok(
                result,
                "Admin dashboard retrieved successfully."));
    }

    /// <summary>
    /// Gets all registered users from the database.
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(
        typeof(ApiResponse<List<AdminUserResponse>>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<AdminUserResponse>>>> GetUsers(
        CancellationToken cancellationToken)
    {
        var result = await _adminService.GetUsersAsync(
            cancellationToken);

        return Ok(
            ApiResponse<List<AdminUserResponse>>.Ok(
                result,
                "Users retrieved successfully."));
=======

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Gets the administrator dashboard with statistics.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<AdminDashboardResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    public async Task<ActionResult<ApiResponse<AdminDashboardResponse>>> GetDashboard()
    {
        var result = await _adminService.GetDashboardAsync();

        return Ok(ApiResponse<AdminDashboardResponse>.Ok(
            result,
            "Admin dashboard retrieved successfully."));
    }

    /// <summary>
    /// Gets all users in the system.
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<List<UserResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetUsers()
    {
        var result = await _adminService.GetUsersAsync();

        return Ok(ApiResponse<List<UserResponse>>.Ok(
            result,
            "Users retrieved successfully."));
>>>>>>> origin/develop
    }

    /// <summary>
    /// Checks database and AI service health.
    /// </summary>
    [HttpGet("system-status")]
<<<<<<< HEAD
    [ProducesResponseType(
        typeof(ApiResponse<AdminSystemStatusResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AdminSystemStatusResponse>>> GetSystemStatus(
        CancellationToken cancellationToken)
    {
        var result = await _adminService.GetSystemStatusAsync(
            cancellationToken);

        return Ok(
            ApiResponse<AdminSystemStatusResponse>.Ok(
                result,
                "System status retrieved successfully."));
    }
}
=======
    [ProducesResponseType(typeof(ApiResponse<SystemStatusResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    public async Task<ActionResult<ApiResponse<SystemStatusResponse>>> GetSystemStatus()
    {
        var result = await _adminService.GetSystemStatusAsync();

        return Ok(ApiResponse<SystemStatusResponse>.Ok(
            result,
            "System status retrieved successfully."));
    }
}
>>>>>>> origin/develop
