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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentResponse>> GetById(Guid id)
    {
        var result = await _appointmentService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("patient/{patientId:guid}")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetByPatient(Guid patientId)
    {
        var result = await _appointmentService.GetByPatientAsync(patientId);
        return Ok(result);
    }

    [HttpGet("doctor/{doctorId:guid}")]
    public async Task<ActionResult<List<AppointmentResponse>>> GetByDoctor(Guid doctorId)
    {
        var result = await _appointmentService.GetByDoctorAsync(doctorId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> Create(CreateAppointmentRequest request)
    {
        var result = await _appointmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<AppointmentResponse>> Cancel(
        Guid id, [FromQuery] string? cancellationReason)
    {
        var result = await _appointmentService.CancelAsync(id, cancellationReason);
        return Ok(result);
    }

    [HttpPut("{id:guid}/reschedule")]
    public async Task<ActionResult<AppointmentResponse>> Reschedule(
        Guid id, RescheduleAppointmentRequest request)
    {
        var result = await _appointmentService.RescheduleAsync(id, request);
        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<ActionResult<AppointmentResponse>> UpdateStatus(
        Guid id, [FromQuery] string status)
    {
        var result = await _appointmentService.UpdateStatusAsync(id, status);
        return Ok(result);
    }
}