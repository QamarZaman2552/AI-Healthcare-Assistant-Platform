using AIHealthcareAssistant.Application.Common.Models;
using AIHealthcareAssistant.Application.Features.Doctors;
using AIHealthcareAssistant.Application.Features.Specialties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<DoctorResponse>>> GetDoctors([FromQuery] DoctorQuery query)
    {
        var result = await _doctorService.GetDoctorsAsync(query);
        return Ok(result);
    }

    [HttpGet("by-status")]
    public async Task<ActionResult<PagedResult<DoctorResponse>>> GetByStatus(
        [FromQuery] bool? isActive, [FromQuery] bool? isVerified,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = new DoctorQuery { IsActive = isActive, IsVerified = isVerified, Page = page, PageSize = pageSize };
        var result = await _doctorService.GetDoctorsAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<DoctorResponse>> GetById(Guid id)
    {
        var result = await _doctorService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<DoctorResponse>> GetByUserId(Guid userId)
    {
        var result = await _doctorService.GetByUserIdAsync(userId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("specialty/{specialtyId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<DoctorResponse>>> GetBySpecialty(Guid specialtyId)
    {
        var result = await _doctorService.GetDoctorsBySpecialtyAsync(specialtyId);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<ActionResult<DoctorResponse>> Create(CreateDoctorRequest request)
    {
        var result = await _doctorService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<ActionResult<DoctorResponse>> Update(Guid id, UpdateDoctorRequest request)
    {
        var result = await _doctorService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<ActionResult<DoctorResponse>> UpdateStatus(Guid id, UpdateDoctorStatusRequest request)
    {
        var result = await _doctorService.UpdateStatusAsync(id, request.IsActive);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/verification")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DoctorResponse>> UpdateVerification(Guid id, UpdateDoctorVerificationRequest request)
    {
        var result = await _doctorService.UpdateVerificationAsync(id, request.IsVerified);
        return Ok(result);
    }

    [HttpGet("{id:guid}/specialties")]
    [AllowAnonymous]
    public async Task<ActionResult<List<SpecialtyResponse>>> GetSpecialties(Guid id)
    {
        var result = await _doctorService.GetSpecialtiesByDoctorAsync(id);
        return Ok(result);
    }

    [HttpPost("{id:guid}/specialties")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<ActionResult<SpecialtyResponse>> AssignSpecialty(Guid id, AssignSpecialtyRequest request)
    {
        var result = await _doctorService.AssignSpecialtyAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/specialties/{specialtyId:guid}")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> RemoveSpecialty(Guid id, Guid specialtyId)
    {
        await _doctorService.RemoveSpecialtyAsync(id, specialtyId);
        return NoContent();
    }
}
