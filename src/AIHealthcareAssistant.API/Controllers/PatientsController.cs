
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<ActionResult<ApiResponse<PatientProfileResponse>>> CreateProfile(
        Guid id,
        [FromBody] PatientProfileRequest request)
    {
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
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<ActionResult<ApiResponse<PatientProfileResponse>>> UpdateProfile(
        Guid id,
        [FromBody] PatientProfileRequest request)
    {
        var result = await _patientService.UpdateProfileAsync(id, request);

        return Ok(ApiResponse<PatientProfileResponse>.Ok(
            result,
            "Patient profile updated successfully."));
    }
}

