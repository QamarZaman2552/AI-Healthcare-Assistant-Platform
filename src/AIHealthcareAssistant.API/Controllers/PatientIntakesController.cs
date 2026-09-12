
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.PatientIntakes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientIntakesController : ControllerBase
{
    private readonly IPatientIntakeService _patientIntakeService;

    public PatientIntakesController(IPatientIntakeService patientIntakeService)
    {
        _patientIntakeService = patientIntakeService;
    }

    /// <summary>
    /// Gets a patient intake by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PatientIntakeResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<PatientIntakeResponse>>> GetById(Guid id)
    {
        var result = await _patientIntakeService.GetByIdAsync(id);

        if (result == null)
            return NotFound(ApiErrorResponse.Error(
                "Patient intake was not found."));

        return Ok(ApiResponse<PatientIntakeResponse>.Ok(
            result,
            "Patient intake retrieved successfully."));
    }

    /// <summary>
    /// Gets all intake records for a patient.
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<PatientIntakeResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<List<PatientIntakeResponse>>>> GetByPatient(
        Guid patientId)
    {
        var result = await _patientIntakeService.GetByPatientAsync(patientId);

        return Ok(ApiResponse<List<PatientIntakeResponse>>.Ok(
            result,
            "Patient intake records retrieved successfully."));
    }

    /// <summary>
    /// Gets the intake associated with an appointment.
    /// </summary>
    [HttpGet("appointment/{appointmentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PatientIntakeResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<PatientIntakeResponse>>> GetByAppointment(
        Guid appointmentId)
    {
        var result = await _patientIntakeService.GetByAppointmentAsync(
            appointmentId);

        if (result == null)
            return NotFound(ApiErrorResponse.Error(
                "Patient intake for the appointment was not found."));

        return Ok(ApiResponse<PatientIntakeResponse>.Ok(
            result,
            "Appointment intake retrieved successfully."));
    }

    /// <summary>
    /// Creates a new patient intake.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PatientIntakeResponse>), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<ActionResult<ApiResponse<PatientIntakeResponse>>> Create(
        [FromBody] CreatePatientIntakeRequest request)
    {
        var result = await _patientIntakeService.CreateAsync(request);

        var response = ApiResponse<PatientIntakeResponse>.Ok(
            result,
            "Patient intake created successfully.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            response);
    }

    /// <summary>
    /// Updates the status of a patient intake.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<PatientIntakeResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<ActionResult<ApiResponse<PatientIntakeResponse>>> UpdateStatus(
        Guid id,
        [FromBody] UpdatePatientIntakeStatusRequest request)
    {
        var result = await _patientIntakeService.UpdateStatusAsync(
            id,
            request.Status);

        return Ok(ApiResponse<PatientIntakeResponse>.Ok(
            result,
            "Patient intake status updated successfully."));
    }
}

