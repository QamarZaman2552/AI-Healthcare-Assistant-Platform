
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Appointments;
using AIHealthcareAssistant.Application.Features.Doctors;
using AIHealthcareAssistant.Application.Features.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIHealthcareAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService;
    private readonly IDoctorService _doctorService;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IPatientService patientService,
        IDoctorService doctorService)
    {
        _appointmentService = appointmentService;
        _patientService = patientService;
        _doctorService = doctorService;
    }

    private Guid GetUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        return userId;
    }

    private bool IsAdmin() => User.IsInRole("Admin");

    private async Task<bool> CanAccessAppointmentAsync(
        Guid patientId,
        Guid doctorId)
    {
        if (IsAdmin())
            return true;

        if (User.IsInRole("Patient"))
        {
            var patient = await _patientService.GetByUserIdAsync(GetUserId());
            return patient != null && patient.Id == patientId;
        }

        if (User.IsInRole("Doctor"))
        {
            var doctor = await _doctorService.GetByUserIdAsync(GetUserId());
            return doctor != null && doctor.Id == doctorId;
        }

        return false;
    }

    private async Task<bool> CanAccessPatientAsync(Guid patientId)
    {
        if (IsAdmin() || User.IsInRole("Doctor"))
            return true;

        var patient = await _patientService.GetByUserIdAsync(GetUserId());
        return patient != null && patient.Id == patientId;
    }

    private async Task<bool> CanAccessDoctorAsync(Guid doctorId)
    {
        if (IsAdmin())
            return true;

        var doctor = await _doctorService.GetByUserIdAsync(GetUserId());
        return doctor != null && doctor.Id == doctorId;
    }

    /// <summary>
    /// Gets an appointment by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> GetById(Guid id)
    {
        var result = await _appointmentService.GetByIdAsync(id);

        if (result == null)
            return NotFound(
                ApiErrorResponse.Error("Appointment was not found."));

        if (!await CanAccessAppointmentAsync(result.PatientId, result.DoctorId))
            return Forbid();

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
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<AppointmentResponse>>>> GetByPatient(
        Guid patientId)
    {
        if (!await CanAccessPatientAsync(patientId))
            return Forbid();

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
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<AppointmentResponse>>>> GetByDoctor(
        Guid doctorId)
    {
        if (!await CanAccessDoctorAsync(doctorId))
            return Forbid();

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
    [Authorize(Roles = "Patient")]
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
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Cancel(
        Guid id,
        [FromBody] CancelAppointmentRequest request)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);

        if (appointment == null)
            return NotFound(
                ApiErrorResponse.Error("Appointment was not found."));

        if (!await CanAccessAppointmentAsync(appointment.PatientId, appointment.DoctorId))
            return Forbid();

        var result = await _appointmentService.CancelAsync(
            id,
            request?.CancellationReason);

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
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Reschedule(
        Guid id,
        [FromBody] RescheduleAppointmentRequest request)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);

        if (appointment == null)
            return NotFound(
                ApiErrorResponse.Error("Appointment was not found."));

        if (!await CanAccessAppointmentAsync(appointment.PatientId, appointment.DoctorId))
            return Forbid();

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
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> UpdateStatus(
        Guid id,
        [FromBody] UpdateAppointmentStatusRequest request)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);

        if (appointment == null)
            return NotFound(
                ApiErrorResponse.Error("Appointment was not found."));

        if (!await CanAccessAppointmentAsync(appointment.PatientId, appointment.DoctorId))
            return Forbid();

        var result = await _appointmentService.UpdateStatusAsync(
            id,
            request.Status);

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
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<AppointmentResponse>>>> GetDoctorDashboardAppointments(
        Guid doctorId,
        [FromQuery] string? status,
        [FromQuery] DateTime? date)
    {
        if (!await CanAccessDoctorAsync(doctorId))
            return Forbid();

        var result = await _appointmentService.GetDoctorDashboardAppointmentsAsync(doctorId, status, date);

        return Ok(
            ApiResponse<List<AppointmentResponse>>.Ok(
                result,
                "Doctor dashboard appointments retrieved successfully."));
    }
}
