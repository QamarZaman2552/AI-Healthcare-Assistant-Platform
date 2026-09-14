
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Availability;
using AIHealthcareAssistant.Application.Features.Doctors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;
    private readonly IDoctorService _doctorService;

    public AvailabilityController(
        IAvailabilityService availabilityService,
        IDoctorService doctorService)
    {
        _availabilityService = availabilityService;
        _doctorService = doctorService;
    }

    /// <summary>
    /// Searches available doctors for a specialty on a specific date.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<List<DoctorResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<List<DoctorResponse>>>> SearchAvailableDoctors(
        [FromQuery] Guid specialtyId,
        [FromQuery] DateOnly date)
    {
        var doctors = await _doctorService.GetDoctorsAsync(new DoctorQuery
        {
            SpecialtyId = specialtyId,
            AvailableOnly = true,
            Page = 1,
            PageSize = 50
        });

        var availableDoctors = new List<DoctorResponse>();
        foreach (var doctor in doctors.Items)
        {
            var slots = await _availabilityService.GetAvailableSlotsAsync(doctor.Id, date);
            if (slots.Any(s => s.IsAvailable))
            {
                availableDoctors.Add(doctor);
            }
        }

        return Ok(ApiResponse<List<DoctorResponse>>.Ok(
            availableDoctors,
            "Available doctors retrieved successfully."));
    }

    /// <summary>
    /// Gets all availability records for a doctor.
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<AvailabilityResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<List<AvailabilityResponse>>>> GetByDoctor(Guid doctorId)
    {
        var result = await _availabilityService.GetByDoctorAsync(doctorId);

        return Ok(ApiResponse<List<AvailabilityResponse>>.Ok(
            result,
            "Doctor availability retrieved successfully."));
    }

    /// <summary>
    /// Gets doctor availability for a specific day.
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}/day/{dayOfWeek}")]
    [ProducesResponseType(typeof(ApiResponse<List<AvailabilityResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<List<AvailabilityResponse>>>> GetByDoctorAndDay(
        Guid doctorId,
        DayOfWeek dayOfWeek)
    {
        var result = await _availabilityService.GetByDoctorAndDayAsync(
            doctorId,
            dayOfWeek);

        return Ok(ApiResponse<List<AvailabilityResponse>>.Ok(
            result,
            "Doctor availability for the selected day retrieved successfully."));
    }

    /// <summary>
    /// Gets available time slots for a doctor on a specific date.
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}/slots/{date}")]
    [ProducesResponseType(typeof(ApiResponse<List<TimeSlotResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<List<TimeSlotResponse>>>> GetAvailableSlots(
        Guid doctorId,
        DateOnly date)
    {
        var result = await _availabilityService.GetAvailableSlotsAsync(
            doctorId,
            date);

        return Ok(ApiResponse<List<TimeSlotResponse>>.Ok(
            result,
            "Available time slots retrieved successfully."));
    }

    /// <summary>
    /// Gets an availability record by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AvailabilityResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<AvailabilityResponse>>> GetById(Guid id)
    {
        var result = await _availabilityService.GetByIdAsync(id);

        if (result == null)
            return NotFound(ApiErrorResponse.Error(
                "Availability record was not found."));

        return Ok(ApiResponse<AvailabilityResponse>.Ok(
            result,
            "Availability retrieved successfully."));
    }

    /// <summary>
    /// Creates a new doctor availability record.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<AvailabilityResponse>), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<ActionResult<ApiResponse<AvailabilityResponse>>> Create(
        [FromBody] CreateAvailabilityRequest request)
    {
        var result = await _availabilityService.CreateAsync(request);

        var response = ApiResponse<AvailabilityResponse>.Ok(
            result,
            "Availability created successfully.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            response);
    }

    /// <summary>
    /// Updates an existing availability record.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(typeof(ApiResponse<AvailabilityResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<ActionResult<ApiResponse<AvailabilityResponse>>> Update(
        Guid id,
        [FromBody] UpdateAvailabilityRequest request)
    {
        var result = await _availabilityService.UpdateAsync(id, request);

        return Ok(ApiResponse<AvailabilityResponse>.Ok(
            result,
            "Availability updated successfully."));
    }

    /// <summary>
    /// Deletes an availability record.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Doctor,Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _availabilityService.DeleteAsync(id);

        return NoContent();
    }
}

