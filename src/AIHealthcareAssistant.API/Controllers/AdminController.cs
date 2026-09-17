using AIHealthcareAssistant.Application.Features.Admin;
using AIHealthcareAssistant.Application.Common.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

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
    }

    /// <summary>
    /// Gets the current system status.
    /// </summary>
    [HttpGet("system-status")]
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
