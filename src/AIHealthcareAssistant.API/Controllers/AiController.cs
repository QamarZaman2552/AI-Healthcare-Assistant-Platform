using AIHealthcareAssistant.Application.Common.Interfaces;
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Ai;
using AIHealthcareAssistant.Application.Features.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIHealthcareAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly IPatientService _patientService;
    private readonly ILogger<AiController> _logger;

    public AiController(
        IAIService aiService,
        IPatientService patientService,
        ILogger<AiController> logger)
    {
        _aiService = aiService;
        _patientService = patientService;
        _logger = logger;
    }

    /// <summary>
    /// Sends a message to the AI healthcare assistant.
    /// </summary>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(AIChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AIChatResponseDto>> Chat(
        [FromBody] AIChatRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var patientId = await GetPatientIdFromTokenAsync();
            request.PatientId = patientId;

            var result = await _aiService.ChatAsync(
                request,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiErrorResponse.Error(ex.Message));
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "AI chat request timed out.");
            return StatusCode(
                StatusCodes.Status408RequestTimeout,
                ApiErrorResponse.Error("AI service request timed out."));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AI provider is unavailable.");
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                ApiErrorResponse.Error("AI service is currently unavailable."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiErrorResponse.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected AI chat error.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiErrorResponse.Error("An unexpected error occurred."));
        }
    }

    /// <summary>
    /// Performs an AI-powered symptom assessment.
    /// </summary>
    [HttpPost("symptom-check")]
    [ProducesResponseType(typeof(AISymptomCheckResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AISymptomCheckResponseDto>> SymptomCheck(
        [FromBody] AISymptomCheckRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var patientId = await GetPatientIdFromTokenAsync();
            request.PatientId = patientId;

            var result = await _aiService.SymptomCheckAsync(
                request,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiErrorResponse.Error(ex.Message));
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "AI symptom check timed out.");
            return StatusCode(
                StatusCodes.Status408RequestTimeout,
                ApiErrorResponse.Error("AI symptom check timed out."));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "AI provider unavailable.");
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                ApiErrorResponse.Error("AI provider unavailable."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected symptom check error.");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiErrorResponse.Error("An unexpected error occurred."));
        }
    }

    /// <summary>
    /// Checks whether the AI provider is available.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Health(
        CancellationToken cancellationToken)
    {
        var healthy = await _aiService.HealthCheckAsync(
            cancellationToken);

        if (!healthy)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    status = "unhealthy",
                    service = "AI"
                });
        }

        return Ok(new
        {
            status = "healthy",
            service = "AI"
        });
    }

    private async Task<Guid> GetPatientIdFromTokenAsync()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString))
            throw new ArgumentException("User ID not found in token");

        var userId = Guid.Parse(userIdString);
        var patient = await _patientService.GetByUserIdAsync(userId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found for the authenticated user");

        return patient.Id;
    }
}
