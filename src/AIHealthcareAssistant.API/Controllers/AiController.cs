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
    private PatientResponse? _authenticatedPatient;

    public AiController(
        IAIService aiService,
        IPatientService patientService,
        ILogger<AiController> logger)
    {
        _aiService = aiService;
        _patientService = patientService;
        _logger = logger;
    }

    private bool IsAdmin() => User.IsInRole("Admin");

    /// <summary>
    /// Sends a message to the AI healthcare assistant with automatic patient context.
    /// </summary>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(AIChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles ="Patient")]
    public async Task<ActionResult<AIChatResponseDto>> Chat(
        [FromBody] AIChatRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var patientId = await GetPatientIdFromTokenAsync();
            request.PatientId = patientId;

            var result = await _aiService.ChatAsync(request, cancellationToken);
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
    /// Gets all conversations for the authenticated patient.
    /// </summary>
    [HttpGet("conversations/patient/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<ConversationResponse>>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<List<ConversationResponse>>>> GetConversationsByPatient(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            var patient = await GetAuthenticatedPatientAsync();

            if (patient.Id != id)
                return NotFound(ApiErrorResponse.Error("Conversations were not found."));
        }

        var result = await _aiService.GetConversationsByPatientAsync(id, cancellationToken);

        return Ok(ApiResponse<List<ConversationResponse>>.Ok(
            result,
            "Conversations retrieved successfully."));
    }

    /// <summary>
    /// Gets a specific conversation with all messages.
    /// </summary>
    [HttpGet("conversations/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ConversationDetailResponse>), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    public async Task<ActionResult<ApiResponse<ConversationDetailResponse>>> GetConversation(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiService.GetConversationByIdAsync(id, cancellationToken);

            if (!IsAdmin())
            {
                var patient = await GetAuthenticatedPatientAsync();

                if (result.PatientId != patient.Id)
                    return NotFound(ApiErrorResponse.Error("Conversation was not found."));
            }

            return Ok(ApiResponse<ConversationDetailResponse>.Ok(
                result,
                "Conversation retrieved successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiErrorResponse.Error(ex.Message));
        }
    }

    /// <summary>
    /// Creates a new conversation for a patient.
    /// </summary>
    [HttpPost("conversations")]
    [ProducesResponseType(typeof(ApiResponse<ConversationResponse>), 201)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<ActionResult<ApiResponse<ConversationResponse>>> CreateConversation(
        [FromBody] CreateConversationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!IsAdmin())
            {
                var patient = await GetAuthenticatedPatientAsync();
                request.PatientId = patient.Id;
            }
            else if (request.PatientId == Guid.Empty)
            {
                return BadRequest(ApiErrorResponse.Error("PatientId is required."));
            }

            var result = await _aiService.CreateConversationAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetConversation),
                new { id = result.Id },
                ApiResponse<ConversationResponse>.Ok(
                    result,
                    "Conversation created successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiErrorResponse.Error(ex.Message));
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
                ApiErrorResponse.Error("AI service is currently unavailable."));
        }

        return Ok(ApiResponse<object>.Ok(
            new { status = "healthy", service = "AI" },
            "AI service is healthy."));
    }

    private async Task<PatientResponse> GetAuthenticatedPatientAsync()
    {
        if (_authenticatedPatient != null)
            return _authenticatedPatient;

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new ArgumentException("User ID not found in token or is invalid");

        var patient = await _patientService.GetByUserIdAsync(userId);
        if (patient == null)
            throw new KeyNotFoundException("Patient not found for the authenticated user");

        _authenticatedPatient = patient;
        return patient;
    }

    private async Task<Guid> GetPatientIdFromTokenAsync()
    {
        var patient = await GetAuthenticatedPatientAsync();
        return patient.Id;
    }
}
