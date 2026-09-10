using AIHealthcareAssistant.Application.Common.Interfaces;
using AIHealthcareAssistant.Application.Features.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIHealthcareAssistant.API.Controllers;

[ApiController]
[Route("api/[controller]")]

public class AiController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<AiController> _logger;

    public AiController(
        IAIService aiService,
        ILogger<AiController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// Sends a message to the AI healthcare assistant.
    /// </summary>
    [HttpPost("chat")]
    [ProducesResponseType(
        typeof(AIChatResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AIChatResponseDto>> Chat(
        [FromBody] AIChatRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiService.ChatAsync(
                request,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(
                ex,
                "AI chat request timed out.");

            return StatusCode(
                StatusCodes.Status408RequestTimeout,
                new
                {
                    message = "AI service request timed out."
                });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "AI provider is unavailable.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message = "AI service is currently unavailable."
                });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected AI chat error.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "An unexpected error occurred."
                });
        }
    }

    /// <summary>
    /// Performs an AI-powered symptom assessment.
    /// </summary>
    [HttpPost("symptom-check")]
    [ProducesResponseType(
        typeof(AISymptomCheckResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AISymptomCheckResponseDto>> SymptomCheck(
        [FromBody] AISymptomCheckRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiService.SymptomCheckAsync(
                request,
                cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(
                ex,
                "AI symptom check timed out.");

            return StatusCode(
                StatusCodes.Status408RequestTimeout,
                new
                {
                    message = "AI service request timed out."
                });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "AI provider unavailable.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message = "AI service is currently unavailable."
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected symptom check error.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "An unexpected error occurred."
                });
        }
    }

    /// <summary>
    /// Checks whether the AI provider is available.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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
}