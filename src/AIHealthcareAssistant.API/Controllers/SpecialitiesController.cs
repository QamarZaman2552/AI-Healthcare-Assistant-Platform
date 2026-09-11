using AIHealthcareAssistant.Application.Features.Specialties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SpecialitiesController : ControllerBase
{
    private readonly ISpecialtyService _specialtyService;

    public SpecialitiesController(ISpecialtyService specialtyService)
    {
        _specialtyService = specialtyService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SpecialtyResponse>>> GetAll()
    {
        var result = await _specialtyService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SpecialtyResponse>> GetById(Guid id)
    {
        var result = await _specialtyService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SpecialtyResponse>> Create(CreateSpecialtyRequest request)
    {
        var result = await _specialtyService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SpecialtyResponse>> Update(Guid id, UpdateSpecialtyRequest request)
    {
        var result = await _specialtyService.UpdateAsync(id, request);
        return Ok(result);
    }
}
