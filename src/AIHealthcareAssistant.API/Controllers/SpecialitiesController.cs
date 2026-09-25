
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Specialties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SpecialitiesController : ControllerBase
{
    private readonly ISpecialtyService _specialtyService;

    public SpecialitiesController(ISpecialtyService specialtyService)
    {
        _specialtyService = specialtyService;
    }

    /// <summary>
    /// Gets all medical specialties.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<SpecialtyResponse>>), 200)]
    public async Task<ActionResult<ApiResponse<List<SpecialtyResponse>>>> GetAll()
    {
        var result = await _specialtyService.GetAllAsync();

        return Ok(ApiResponse<List<SpecialtyResponse>>.Ok(
            result,
            "Specialties retrieved successfully."));
    }

    /// <summary>
    /// Gets a specialty by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SpecialtyResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<ActionResult<ApiResponse<SpecialtyResponse>>> GetById(Guid id)
    {
        var result = await _specialtyService.GetByIdAsync(id);

        if (result == null)
            return NotFound(ApiErrorResponse.Error(
                "Specialty was not found."));

        return Ok(ApiResponse<SpecialtyResponse>.Ok(
            result,
            "Specialty retrieved successfully."));
    }

    /// <summary>
    /// Creates a new medical specialty.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SpecialtyResponse>), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<ActionResult<ApiResponse<SpecialtyResponse>>> Create(
        [FromBody] CreateSpecialtyRequest request)
    {
        var result = await _specialtyService.CreateAsync(request);

        var response = ApiResponse<SpecialtyResponse>.Ok(
            result,
            "Specialty created successfully.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            response);
    }

    /// <summary>
    /// Updates an existing medical specialty.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SpecialtyResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<ActionResult<ApiResponse<SpecialtyResponse>>> Update(
        Guid id,
        [FromBody] UpdateSpecialtyRequest request)
    {
        var result = await _specialtyService.UpdateAsync(id, request);

        return Ok(ApiResponse<SpecialtyResponse>.Ok(
            result,
            "Specialty updated successfully."));
    }

    /// <summary>
    /// Deletes a medical specialty.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _specialtyService.DeleteAsync(id);

        return NoContent();
    }
}

