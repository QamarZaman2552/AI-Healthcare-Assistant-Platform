
using AIHealthcareAssistant.Application.Common.Models;
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Doctors;
using AIHealthcareAssistant.Application.Features.Specialties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

