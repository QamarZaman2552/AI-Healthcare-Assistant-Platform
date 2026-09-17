using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Admin;
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
    }

    /// <summary>
    /// Checks database and AI service health.
    /// </summary>
    [HttpGet("system-status")]
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