
using AIHealthcareAssistant.Application.Common.Models;
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Doctors;
using AIHealthcareAssistant.Application.Features.Specialties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIHealthcareAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    private Guid GetUserId()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(value) || !Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token.");

        return userId;
    }

    private async Task<bool> CanAccessDoctorAsync(Guid doctorId)
    {
        if (User.IsInRole("Admin"))
            return true;

        var doctor = await _doctorService.GetByUserIdAsync(GetUserId());
        return doctor != null && doctor.Id == doctorId;
    }

    /// <summary>
    /// Gets a paginated list of doctors.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DoctorResponse>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResult<DoctorResponse>>>> GetDoctors(
        [FromQuery] DoctorQuery query)
    {
        var result = await _doctorService.GetDoctorsAsync(query);

        return Ok(ApiResponse<PagedResult<DoctorResponse>>.Ok(
            result,
            "Doctors retrieved successfully."));
    }

    /// <summary>
    /// Gets doctors filtered by active and verification status.
    /// </summary>
    [HttpGet("by-status")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DoctorResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<PagedResult<DoctorResponse>>>> GetByStatus(
        [FromQuery] bool? isActive,
        [FromQuery] bool? isVerified,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new DoctorQuery
        {
            IsActive = isActive,
            IsVerified = isVerified,
            Page = page,
            PageSize = pageSize
        };

        var result = await _doctorService.GetDoctorsAsync(query);

        return Ok(ApiResponse<PagedResult<DoctorResponse>>.Ok(
            result,
            "Doctors filtered successfully."));
    }

    /// <summary>
    /// Gets a doctor by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<DoctorResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<ActionResult<ApiResponse<DoctorResponse>>> GetById(Guid id)
    {
        var result = await _doctorService.GetByIdAsync(id);

        if (result == null)
            return NotFound(ApiErrorResponse.Error(
                "Doctor was not found."));

        return Ok(ApiResponse<DoctorResponse>.Ok(
            result,
            "Doctor retrieved successfully."));
    }

    /// <summary>
    /// Gets a doctor by user ID.
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DoctorResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<DoctorResponse>>> GetByUserId(Guid userId)
    {
        var result = await _doctorService.GetByUserIdAsync(userId);

        if (result == null)
            return NotFound(ApiErrorResponse.Error(
                "Doctor was not found for the specified user."));

        return Ok(ApiResponse<DoctorResponse>.Ok(
            result,
            "Doctor retrieved successfully."));
    }

    /// <summary>
    /// Gets all doctors belonging to a specialty.
    /// </summary>
    [HttpGet("specialty/{specialtyId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<DoctorResponse>>), 200)]
    public async Task<ActionResult<ApiResponse<List<DoctorResponse>>>> GetBySpecialty(
        Guid specialtyId)
    {
        var result = await _doctorService.GetDoctorsBySpecialtyAsync(specialtyId);

        return Ok(ApiResponse<List<DoctorResponse>>.Ok(
            result,
            "Doctors for the specialty retrieved successfully."));
    }

    /// <summary>
    /// Creates a doctor.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<DoctorResponse>), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<ActionResult<ApiResponse<DoctorResponse>>> Create(
        [FromBody] CreateDoctorRequest request)
    {
        var result = await _doctorService.CreateAsync(request);

        var response = ApiResponse<DoctorResponse>.Ok(
            result,
            "Doctor created successfully.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            response);
    }

    /// <summary>
    /// Updates doctor information.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<DoctorResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<ActionResult<ApiResponse<DoctorResponse>>> Update(
        Guid id,
        [FromBody] UpdateDoctorRequest request)
    {
        var result = await _doctorService.UpdateAsync(id, request);

        return Ok(ApiResponse<DoctorResponse>.Ok(
            result,
            "Doctor updated successfully."));
    }

    /// <summary>
    /// Updates doctor active status.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<DoctorResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<ActionResult<ApiResponse<DoctorResponse>>> UpdateStatus(
        Guid id,
        [FromBody] UpdateDoctorStatusRequest request)
    {
        var result = await _doctorService.UpdateStatusAsync(
            id,
            request.IsActive);

        return Ok(ApiResponse<DoctorResponse>.Ok(
            result,
            "Doctor status updated successfully."));
    }

    /// <summary>
    /// Updates doctor verification status.
    /// </summary>
    [HttpPatch("{id:guid}/verification")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<DoctorResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<ActionResult<ApiResponse<DoctorResponse>>> UpdateVerification(
        Guid id,
        [FromBody] UpdateDoctorVerificationRequest request)
    {
        var result = await _doctorService.UpdateVerificationAsync(
            id,
            request.IsVerified);

        return Ok(ApiResponse<DoctorResponse>.Ok(
            result,
            "Doctor verification status updated successfully."));
    }

    /// <summary>
    /// Gets all specialties assigned to a doctor.
    /// </summary>
    [HttpGet("{id:guid}/specialties")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<SpecialtyResponse>>), 200)]
    public async Task<ActionResult<ApiResponse<List<SpecialtyResponse>>>> GetSpecialties(
        Guid id)
    {
        var result = await _doctorService.GetSpecialtiesByDoctorAsync(id);

        return Ok(ApiResponse<List<SpecialtyResponse>>.Ok(
            result,
            "Doctor specialties retrieved successfully."));
    }

    /// <summary>
    /// Assigns a specialty to a doctor.
    /// </summary>
    [HttpPost("{id:guid}/specialties")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<SpecialtyResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<ActionResult<ApiResponse<SpecialtyResponse>>> AssignSpecialty(
        Guid id,
        [FromBody] AssignSpecialtyRequest request)
    {
        var result = await _doctorService.AssignSpecialtyAsync(id, request);

        return Ok(ApiResponse<SpecialtyResponse>.Ok(
            result,
            "Specialty assigned successfully."));
    }

    /// <summary>
    /// Gets doctor dashboard stats.
    /// </summary>
    [HttpGet("dashboard/stats")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<DoctorDashboardResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<DoctorDashboardResponse>>> GetDashboardStats(Guid id)
    {
        if (!await CanAccessDoctorAsync(id))
            return Forbid();

        var result = await _doctorService.GetDashboardStatsAsync(id);

        if (result == null)
            return NotFound(ApiErrorResponse.Error(
                "Doctor dashboard stats were not found."));

        return Ok(ApiResponse<DoctorDashboardResponse>.Ok(
            result,
            "Doctor dashboard stats retrieved successfully."));
    }

    /// <summary>
    /// Gets doctor appointments by doctor ID for a specific date.
    /// </summary>
    [HttpGet("{id:guid}/appointments")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<DoctorAppointmentResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<List<DoctorAppointmentResponse>>>> GetAppointmentsByDoctor(
        Guid id,
        [FromQuery] string date = "today")
    {
        if (!await CanAccessDoctorAsync(id))
            return Forbid();

        DateOnly parsedDate;

        if (string.Equals(date, "today", StringComparison.OrdinalIgnoreCase))
        {
            parsedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        }
        else if (!DateOnly.TryParse(date, out parsedDate))
        {
            return BadRequest(ApiErrorResponse.Error(
                "Invalid date format. Use yyyy-MM-dd or 'today'."));
        }

        var result = await _doctorService.GetDoctorAppointmentsAsync(id, parsedDate);

        return Ok(ApiResponse<List<DoctorAppointmentResponse>>.Ok(
            result,
            "Doctor appointments retrieved successfully."));
    }

    /// <summary>
    /// Updates doctor profile information.
    /// </summary>
    [HttpPatch("profile")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<DoctorResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<ActionResult<ApiResponse<DoctorResponse>>> UpdateProfile(
        [FromBody] UpdateDoctorRequest request)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var doctor = await _doctorService.GetByUserIdAsync(userId);

        if (doctor == null)
            return NotFound(ApiErrorResponse.Error(
                "Doctor profile was not found."));

        var result = await _doctorService.UpdateAsync(doctor.Id, request);

        return Ok(ApiResponse<DoctorResponse>.Ok(
            result,
            "Doctor profile updated successfully."));
    }

    /// <summary>
    /// Removes a specialty from a doctor.
    /// </summary>
    [HttpDelete("{id:guid}/specialties/{specialtyId:guid}")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> RemoveSpecialty(
        Guid id,
        Guid specialtyId)
    {
        await _doctorService.RemoveSpecialtyAsync(id, specialtyId);

        return NoContent();
    }
}

