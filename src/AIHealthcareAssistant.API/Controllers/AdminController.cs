
using AIHealthcareAssistant.Application.Common.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    /// <summary>
    /// Gets the administrator dashboard.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    public IActionResult GetDashboard()
    {
        var data = new
        {
            status = "active",
            message = "Administrator dashboard is available."
        };

        return Ok(
            ApiResponse<object>.Ok(
                data,
                "Admin dashboard retrieved successfully."));
    }

    /// <summary>
    /// Gets the users available to the administrator.
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    public IActionResult GetUsers()
    {
        var data = new
        {
            message = "User management endpoint is available."
        };

        return Ok(
            ApiResponse<object>.Ok(
                data,
                "Users information retrieved successfully."));
    }

    /// <summary>
    /// Gets the current system status.
    /// </summary>
    [HttpGet("system-status")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    public IActionResult GetSystemStatus()
    {
        var data = new
        {
            status = "operational",
            service = "AI Healthcare Assistant API"
        };

        return Ok(
            ApiResponse<object>.Ok(
                data,
                "System status retrieved successfully."));
    }
}

