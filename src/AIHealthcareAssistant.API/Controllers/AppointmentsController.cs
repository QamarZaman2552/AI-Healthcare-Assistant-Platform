
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Appointments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    /// <summary>
    /// Gets an appointment by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> GetById(Guid id)
    {
        var result = await _appointmentService.GetByIdAsync(id);

        if (result == null)
            return NotFound(
                ApiErrorResponse.Error("Appointment was not found."));

        return Ok(
            ApiResponse<AppointmentResponse>.Ok(
                result,
                "Appointment retrieved successfully."));
    }

    /// <summary>
    /// Gets all appointments for a patient.
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<AppointmentResponse>>>> GetByPatient(
        Guid patientId)
    {
        var result = await _appointmentService.GetByPatientAsync(patientId);

        return Ok(
            ApiResponse<List<AppointmentResponse>>.Ok(
                result,
                "Patient appointments retrieved successfully."));
    }

    /// <summary>
    /// Gets all appointments for a doctor.
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<AppointmentResponse>>>> GetByDoctor(
        Guid doctorId)
    {
        var result = await _appointmentService.GetByDoctorAsync(doctorId);

        return Ok(
            ApiResponse<List<AppointmentResponse>>.Ok(
                result,
                "Doctor appointments retrieved successfully."));
    }

    /// <summary>
    /// Creates a new appointment.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Create(
        [FromBody] CreateAppointmentRequest request)
    {
        var result = await _appointmentService.CreateAsync(request);

        var response = ApiResponse<AppointmentResponse>.Ok(
            result,
            "Appointment created successfully.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            response);
    }

    /// <summary>
    /// Cancels an existing appointment.
    /// </summary>
    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Cancel(
        Guid id,
        [FromQuery] string? cancellationReason)
    {
        var result = await _appointmentService.CancelAsync(
            id,
            cancellationReason);

        return Ok(
            ApiResponse<AppointmentResponse>.Ok(
                result,
                "Appointment cancelled successfully."));
    }

    /// <summary>
    /// Reschedules an existing appointment.
    /// </summary>
    [HttpPut("{id:guid}/reschedule")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Reschedule(
        Guid id,
        [FromBody] RescheduleAppointmentRequest request)
    {
        var result = await _appointmentService.RescheduleAsync(
            id,
            request);

        return Ok(
            ApiResponse<AppointmentResponse>.Ok(
                result,
                "Appointment rescheduled successfully."));
    }

    /// <summary>
    /// Updates the status of an appointment.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> UpdateStatus(
        Guid id,
        [FromQuery] string status)
    {
        var result = await _appointmentService.UpdateStatusAsync(
            id,
            status);

        return Ok(
            ApiResponse<AppointmentResponse>.Ok(
                result,
                "Appointment status updated successfully."));
    }


    /// <summary>
    /// Gets doctor appointments filtered by status or date range.
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}/dashboard")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<AppointmentResponse>>>> GetDoctorDashboardAppointments(
        Guid doctorId,
        [FromQuery] string? status,
        [FromQuery] DateTime? date)
    {
        var result = await _appointmentService.GetDoctorDashboardAppointmentsAsync(doctorId, status, date);

        return Ok(
            ApiResponse<List<AppointmentResponse>>.Ok(
                result,
                "Doctor dashboard appointments retrieved successfully."));
    }
}

