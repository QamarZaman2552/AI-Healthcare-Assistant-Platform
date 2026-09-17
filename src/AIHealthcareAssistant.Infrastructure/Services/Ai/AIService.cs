using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AIHealthcareAssistant.Application.Common.Interfaces;
using AIHealthcareAssistant.Application.Features.Ai;
using AIHealthcareAssistant.Domain.Entities;
using AIHealthcareAssistant.Domain.Enums;
using AIHealthcareAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIHealthcareAssistant.Infrastructure.Services.Ai;

public sealed class AIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly AISettings _settings;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<AIService> _logger;

    public AIService(
        HttpClient httpClient,
        IOptions<AISettings> settings,
        AppDbContext dbContext,
        ILogger<AIService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<AIChatResponseDto> ChatAsync(
        AIChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.PatientId == Guid.Empty)
            throw new ArgumentException("PatientId is required.");

        if (string.IsNullOrWhiteSpace(request.Message))
            throw new ArgumentException("Message is required.");

        AIConversation conversation;

        if (request.ConversationId.HasValue)
        {
            conversation = await _dbContext.AIConversations
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(
                    x => x.Id == request.ConversationId.Value &&
                         x.PatientId == request.PatientId,
                    cancellationToken)
                ?? throw new KeyNotFoundException(
                    "AI conversation was not found.");
        }
        else
        {
            conversation = new AIConversation
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                AppointmentId = request.AppointmentId,
                Title = request.Message.Length > 100
                    ? request.Message[..100]
                    : request.Message,
                Status = ConversationStatus.Active,
                StartedAt = DateTime.UtcNow
            };

            await _dbContext.AIConversations.AddAsync(
                conversation,
                cancellationToken);
        }

        var nextSequence = conversation.Messages.Count == 0
            ? 1
            : conversation.Messages.Max(x => x.SequenceNumber) + 1;

        var userMessage = new AIConversationMessage
        {
            Id = Guid.NewGuid(),
            AIConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = request.Message,
            SequenceNumber = nextSequence
        };

        await _dbContext.AIConversationMessages.AddAsync(
            userMessage,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Dynamic Context Injection
        var patientContext = await BuildPatientContextAsync(request.PatientId, cancellationToken);

        var providerMessages = new List<AIProviderMessage>
        {
            new()
            {
                Role = "system",
                Content =
                    $"""
                    You are an AI healthcare assistant.

                    Patient Context:
                    {patientContext}

                    Provide general health information only.
                    Do not provide a definitive medical diagnosis.
                    Do not prescribe medication or dosage.

                    If the user's symptoms suggest a medical emergency,
                    clearly recommend immediate professional medical attention.

                    Keep responses clear, concise and understandable.
                    """
            }
        };

        var previousMessages = conversation.Messages
            .OrderBy(x => x.SequenceNumber)
            .TakeLast(20);

        foreach (var message in previousMessages)
        {
            providerMessages.Add(new AIProviderMessage
            {
                Role = MapRole(message.Role),
                Content = message.Content
            });
        }

        var providerRequest = new AIProviderRequest
        {
            Model = _settings.Model,
            Temperature = 0.2,
            Messages = providerMessages
        };

        var providerResponse = await SendToProviderAsync(
            providerRequest,
            cancellationToken);

        var aiMessage = providerResponse?
            .Choices?
            .FirstOrDefault()?
            .Message?
            .Content;

        if (string.IsNullOrWhiteSpace(aiMessage))
        {
            throw new InvalidOperationException(
                "AI provider returned an empty response.");
        }

        var assistantMessage = new AIConversationMessage
        {
            Id = Guid.NewGuid(),
            AIConversationId = conversation.Id,
            Role = MessageRole.Assistant,
            Content = aiMessage,
            SequenceNumber = nextSequence + 1
        };

        await _dbContext.AIConversationMessages.AddAsync(
            assistantMessage,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AIChatResponseDto
        {
            ConversationId = conversation.Id,
            Message = aiMessage
        };
    }

    public async Task<AISymptomCheckResponseDto> SymptomCheckAsync(
        AISymptomCheckRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.PatientId == Guid.Empty)
            throw new ArgumentException("PatientId is required.");

        if (string.IsNullOrWhiteSpace(request.Symptoms))
            throw new ArgumentException("Symptoms are required.");

        var patientContext = await BuildPatientContextAsync(request.PatientId, cancellationToken);

        var prompt = $"""
            You are a healthcare symptom assessment assistant.

            Patient Database Medical Context:
            {patientContext}

            Current Input Details:
            Age: {request.Age}
            Gender: {request.Gender ?? "Not provided"}

            Symptoms:
            {request.Symptoms}

            Medical history:
            {request.MedicalHistory ?? "Not provided"}

            Current medications:
            {request.CurrentMedications ?? "Not provided"}

            Allergies:
            {request.Allergies ?? "Not provided"}

            Analyze the information and return the response
            using exactly these sections:

            Summary:
            Possible Conditions:
            Recommended Action:
            Urgency:
            Emergency: Yes or No

            Important:
            Do not provide a definitive diagnosis.
            Do not prescribe medication.
            If the symptoms may indicate an emergency,
            recommend immediate professional medical attention.
            """;

        var chatRequest = new AIChatRequestDto
        {
            PatientId = request.PatientId,
            Message = prompt
        };

        var response = await ChatAsync(
            chatRequest,
            cancellationToken);

        return ParseSymptomResponse(response.Message);
    }

    public async Task<bool> HealthCheckAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "");

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "AI provider health check failed.");

            return false;
        }
    }

    private async Task<string> BuildPatientContextAsync(Guid patientId, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == patientId || p.UserId == patientId, cancellationToken);

        if (patient == null)
            return "No registered patient context available.";

        var profile = await _dbContext.PatientProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PatientId == patient.Id, cancellationToken);

        var latestIntake = await _dbContext.PatientIntakes
            .AsNoTracking()
            .Where(i => i.PatientId == patient.Id)
            .OrderByDescending(i => i.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine($"Patient Name: {patient.User?.FirstName} {patient.User?.LastName}");

        if (profile != null)
        {
            sb.AppendLine($"Medical History: {profile.MedicalHistory ?? "N/A"}");
            sb.AppendLine($"Allergies: {profile.Allergies ?? "None"}");
            sb.AppendLine($"Current Medications: {profile.CurrentMedications ?? "None"}");
        }

        if (latestIntake != null)
        {
            sb.AppendLine($"Recent Intake Chief Complaint: {latestIntake.ChiefComplaint}");
            sb.AppendLine($"Recent Intake Symptoms: {latestIntake.SymptomsDescription}");
        }

        return sb.ToString();
    }

    private async Task<AIProviderResponse?> SendToProviderAsync(
        AIProviderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                "chat/completions");

            httpRequest.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    _settings.ApiKey);

            httpRequest.Content = JsonContent.Create(request);

            using var response = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content
                    .ReadAsStringAsync(cancellationToken);

                _logger.LogError(
                    "AI provider returned HTTP {StatusCode}. Response: {Response}",
                    (int)response.StatusCode,
                    error);

                throw new HttpRequestException(
                    $"AI provider returned {(int)response.StatusCode}.");
            }

            return await response.Content
                .ReadFromJsonAsync<AIProviderResponse>(
                    cancellationToken: cancellationToken);
        }
        catch (TaskCanceledException ex)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "AI provider request timed out.");

            throw new TimeoutException(
                "AI provider request timed out.",
                ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Invalid JSON received from AI provider.");

            throw new InvalidOperationException(
                "Invalid response received from AI provider.",
                ex);
        }
    }

    private static string MapRole(MessageRole role)
    {
        return role switch
        {
            MessageRole.User => "user",
            MessageRole.Assistant => "assistant",
            _ => "user"
        };
    }

    private static AISymptomCheckResponseDto ParseSymptomResponse(
        string response)
    {
        var summary = ExtractSection(
            response,
            "Summary:",
            "Possible Conditions:");

        var conditions = ExtractSection(
            response,
            "Possible Conditions:",
            "Recommended Action:");

        var action = ExtractSection(
            response,
            "Recommended Action:",
            "Urgency:");

        var urgency = ExtractSection(
            response,
            "Urgency:",
            "Emergency:");

        var emergency = ExtractSection(
            response,
            "Emergency:",
            null);

        return new AISymptomCheckResponseDto
        {
            Summary = summary,
            PossibleConditions = conditions,
            RecommendedAction = action,
            Urgency = urgency,
            RequiresEmergencyCare =
                emergency.Contains(
                    "yes",
                    StringComparison.OrdinalIgnoreCase)
        };
    }

    private static string ExtractSection(
        string text,
        string start,
        string? end)
    {
        var startIndex = text.IndexOf(
            start,
            StringComparison.OrdinalIgnoreCase);

        if (startIndex < 0)
            return string.Empty;

        startIndex += start.Length;

        var endIndex = end == null
            ? text.Length
            : text.IndexOf(
                end,
                startIndex,
                StringComparison.OrdinalIgnoreCase);

        if (endIndex < 0)
            endIndex = text.Length;

        return text[startIndex..endIndex].Trim();
    }
}