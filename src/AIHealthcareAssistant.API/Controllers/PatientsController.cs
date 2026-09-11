using AIHealthcareAssistant.Application.Features.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PatientResponse>> GetById(Guid id)
    {
        var result = await _patientService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<PatientResponse>> GetByUserId(Guid userId)
    {
        var result = await _patientService.GetByUserIdAsync(userId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:guid}/profile")]
    public async Task<ActionResult<PatientProfileResponse>> GetProfile(Guid id)
    {
        var result = await _patientService.GetProfileAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("{id:guid}/profile")]
    public async Task<ActionResult<PatientProfileResponse>> CreateProfile(Guid id, PatientProfileRequest request)
    {
        var result = await _patientService.CreateProfileAsync(id, request);
        return CreatedAtAction(nameof(GetProfile), new { id }, result);
    }

    [HttpPut("{id:guid}/profile")]
    public async Task<ActionResult<PatientProfileResponse>> UpdateProfile(Guid id, PatientProfileRequest request)
    {
        var result = await _patientService.UpdateProfileAsync(id, request);
        return Ok(result);
    }
}
