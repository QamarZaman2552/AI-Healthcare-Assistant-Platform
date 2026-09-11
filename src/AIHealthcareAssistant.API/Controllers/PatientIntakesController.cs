using AIHealthcareAssistant.Application.Features.PatientIntakes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PatientIntakesController : ControllerBase
{
    private readonly IPatientIntakeService _patientIntakeService;

    public PatientIntakesController(IPatientIntakeService patientIntakeService)
    {
        _patientIntakeService = patientIntakeService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PatientIntakeResponse>> GetById(Guid id)
    {
        var result = await _patientIntakeService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("patient/{patientId:guid}")]
    public async Task<ActionResult<List<PatientIntakeResponse>>> GetByPatient(Guid patientId)
    {
        var result = await _patientIntakeService.GetByPatientAsync(patientId);
        return Ok(result);
    }

    [HttpGet("appointment/{appointmentId:guid}")]
    public async Task<ActionResult<PatientIntakeResponse>> GetByAppointment(Guid appointmentId)
    {
        var result = await _patientIntakeService.GetByAppointmentAsync(appointmentId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PatientIntakeResponse>> Create(CreatePatientIntakeRequest request)
    {
        var result = await _patientIntakeService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<ActionResult<PatientIntakeResponse>> UpdateStatus(Guid id, UpdatePatientIntakeStatusRequest request)
    {
        var result = await _patientIntakeService.UpdateStatusAsync(id, request.Status);
        return Ok(result);
    }
}
