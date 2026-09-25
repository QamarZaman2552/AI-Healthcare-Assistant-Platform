
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIHealthcareAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    private Guid GetUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        return userId;
    }

    private async Task<bool> CanAccessPatientAsync(Guid patientId)
    {
        if (User.IsInRole("Admin") || User.IsInRole("Doctor"))
            return true;

        var patient = await _patientService.GetByUserIdAsync(GetUserId());
        return patient != null && patient.Id == patientId;
    }

    /// <summary>
    /// Registers the current user as a patient.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<PatientResponse>), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<ActionResult<ApiResponse<PatientResponse>>> Register()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var result = await _patientService.RegisterAsync(userId);

        var response = ApiResponse<PatientResponse>.Ok(
            result,
            "Patient registered successfully.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            response);
    }

    /// <summary>
    /// Gets all patients.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(typeof(ApiResponse<List<PatientResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    public async Task<ActionResult<ApiResponse<List<PatientResponse>>>> GetAll()
    {
        var result = await _patientService.GetAllAsync();

        return Ok(ApiResponse<List<PatientResponse>>.Ok(
            result,
            "Patients retrieved successfully."));
    }

    /// <summary>
    /// Gets a patient by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PatientResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<PatientResponse>>> GetById(Guid id)
    {
        var result = await _patientService.GetByIdAsync(id);

        if (result == null)
            return NotFound(ApiErrorResponse.Error(
                "Patient was not found."));

        return Ok(ApiResponse<PatientResponse>.Ok(
            result,
            "Patient retrieved successfully."));
    }

    /// <summary>
    /// Gets a patient by user ID.
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PatientResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<PatientResponse>>> GetByUserId(Guid userId)
    {
        var result = await _patientService.GetByUserIdAsync(userId);

        if (result == null)
            return NotFound(ApiErrorResponse.Error(
                "Patient was not found for the specified user."));

        return Ok(ApiResponse<PatientResponse>.Ok(
            result,
            "Patient retrieved successfully."));
    }

    /// <summary>
    /// Gets a patient's profile.
    /// </summary>
    [HttpGet("{id:guid}/profile")]
    [ProducesResponseType(typeof(ApiResponse<PatientProfileResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<PatientProfileResponse>>> GetProfile(Guid id)
    {
        var result = await _patientService.GetProfileAsync(id);

        if (result == null)
            return NotFound(ApiErrorResponse.Error(
                "Patient profile was not found."));

        return Ok(ApiResponse<PatientProfileResponse>.Ok(
            result,
            "Patient profile retrieved successfully."));
    }

    /// <summary>
    /// Creates a patient's profile.
    /// </summary>
    [HttpPost("{id:guid}/profile")]
    [ProducesResponseType(typeof(ApiResponse<PatientProfileResponse>), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<ActionResult<ApiResponse<PatientProfileResponse>>> CreateProfile(
        Guid id,
        [FromBody] PatientProfileRequest request)
    {
        if (!await CanAccessPatientAsync(id))
            return StatusCode(403, ApiErrorResponse.Forbidden("Access denied."));

        var result = await _patientService.CreateProfileAsync(id, request);

        var response = ApiResponse<PatientProfileResponse>.Ok(
            result,
            "Patient profile created successfully.");

        return CreatedAtAction(
            nameof(GetProfile),
            new { id },
            response);
    }

    /// <summary>
    /// Updates a patient's profile.
    /// </summary>
    [HttpPut("{id:guid}/profile")]
    [ProducesResponseType(typeof(ApiResponse<PatientProfileResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<ActionResult<ApiResponse<PatientProfileResponse>>> UpdateProfile(
        Guid id,
        [FromBody] PatientProfileRequest request)
    {
        if (!await CanAccessPatientAsync(id))
            return StatusCode(403, ApiErrorResponse.Forbidden("Access denied."));

        var result = await _patientService.UpdateProfileAsync(id, request);

        return Ok(ApiResponse<PatientProfileResponse>.Ok(
            result,
            "Patient profile updated successfully."));
    }

    /// <summary>
    /// Gets patient dashboard stats.
    /// </summary>
    [HttpGet("dashboard/stats")]
    [ProducesResponseType(typeof(ApiResponse<PatientDashboardResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<PatientDashboardResponse>>> GetDashboardStats(Guid id)
    {
        var result = await _patientService.GetDashboardStatsAsync(id);

        if (result == null)
            return NotFound(ApiErrorResponse.Error(
                "Patient dashboard stats were not found."));

        return Ok(ApiResponse<PatientDashboardResponse>.Ok(
            result,
            "Patient dashboard stats retrieved successfully."));
    }

    /// <summary>
    /// Gets patient appointment history.
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [ProducesResponseType(typeof(ApiResponse<PatientHistoryResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<PatientHistoryResponse>>> GetHistory(Guid id)
    {
        var result = await _patientService.GetHistoryAsync(id);

        if (result == null)
            return NotFound(ApiErrorResponse.Error(
                "Patient was not found."));

        return Ok(ApiResponse<PatientHistoryResponse>.Ok(
            result,
            "Patient history retrieved successfully."));
    }
}

