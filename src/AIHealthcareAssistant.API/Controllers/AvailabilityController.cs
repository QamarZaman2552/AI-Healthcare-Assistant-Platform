using AIHealthcareAssistant.Application.Features.Availability;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;

    public AvailabilityController(IAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    [HttpGet("doctor/{doctorId:guid}")]
    public async Task<ActionResult<List<AvailabilityResponse>>> GetByDoctor(Guid doctorId)
    {
        var result = await _availabilityService.GetByDoctorAsync(doctorId);
        return Ok(result);
    }

    [HttpGet("doctor/{doctorId:guid}/day/{dayOfWeek}")]
    public async Task<ActionResult<List<AvailabilityResponse>>> GetByDoctorAndDay(
        Guid doctorId, DayOfWeek dayOfWeek)
    {
        var result = await _availabilityService.GetByDoctorAndDayAsync(doctorId, dayOfWeek);
        return Ok(result);
    }

    [HttpGet("doctor/{doctorId:guid}/slots/{date}")]
    public async Task<ActionResult<List<TimeSlotResponse>>> GetAvailableSlots(
        Guid doctorId, DateOnly date)
    {
        var result = await _availabilityService.GetAvailableSlotsAsync(doctorId, date);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AvailabilityResponse>> GetById(Guid id)
    {
        var result = await _availabilityService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<ActionResult<AvailabilityResponse>> Create(CreateAvailabilityRequest request)
    {
        var result = await _availabilityService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<ActionResult<AvailabilityResponse>> Update(
        Guid id, UpdateAvailabilityRequest request)
    {
        var result = await _availabilityService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _availabilityService.DeleteAsync(id);
        return NoContent();
    }
}