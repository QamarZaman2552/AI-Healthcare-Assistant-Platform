using AIHealthcareAssistant.API.Controllers;
using AIHealthcareAssistant.Application.Common.Interfaces;
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Ai;
using AIHealthcareAssistant.Application.Features.Patients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace AIHealthcareAssistant.Tests.Controllers;

public class AiControllerTests
{
    private readonly Mock<IAIService> _aiServiceMock;
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly Mock<ILogger<AiController>> _loggerMock;
    private readonly AiController _controller;
    private readonly PatientResponse _authenticatedPatient;

    public AiControllerTests()
    {
        _aiServiceMock = new Mock<IAIService>();
        _patientServiceMock = new Mock<IPatientService>();
        _loggerMock = new Mock<ILogger<AiController>>();
        _controller = new AiController(_aiServiceMock.Object, _patientServiceMock.Object, _loggerMock.Object);

        _authenticatedPatient = new PatientResponse
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FullName = "Test Patient",
            Email = "test@test.com"
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                }, "TestAuth"))
            }
        };

        _patientServiceMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(_authenticatedPatient);
    }

    [Fact]
    public async Task Chat_ValidRequest_ReturnsOk()
    {
        var request = new AIChatRequestDto
        {
            Message = "I have a headache"
        };

        var response = new AIChatResponseDto
        {
            ConversationId = Guid.NewGuid(),
            Message = "For headaches, you can try rest and hydration..."
        };

        _aiServiceMock
            .Setup(x => x.ChatAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Chat(request, CancellationToken.None);

        _aiServiceMock.Verify(x => x.ChatAsync(
            It.Is<AIChatRequestDto>(r => r.PatientId == _authenticatedPatient.Id),
            It.IsAny<CancellationToken>()));

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var chatResponse = Assert.IsType<AIChatResponseDto>(okResult.Value);
        Assert.NotNull(chatResponse);
        Assert.Equal(response.Message, chatResponse.Message);
    }

    [Fact]
    public async Task Chat_EmptyMessage_ReturnsBadRequest()
    {
        var request = new AIChatRequestDto
        {
            PatientId = Guid.NewGuid(),
            Message = ""
        };

        _aiServiceMock
            .Setup(x => x.ChatAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Message is required."));

        var result = await _controller.Chat(request, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Chat_Timeout_Returns408()
    {
        var request = new AIChatRequestDto
        {
            PatientId = Guid.NewGuid(),
            Message = "Hello"
        };

        _aiServiceMock
            .Setup(x => x.ChatAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("AI provider request timed out."));

        var result = await _controller.Chat(request, CancellationToken.None);

        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(408, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Chat_AiUnavailable_Returns503()
    {
        var request = new AIChatRequestDto
        {
            PatientId = Guid.NewGuid(),
            Message = "Hello"
        };

        _aiServiceMock
            .Setup(x => x.ChatAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("AI provider returned 503."));

        var result = await _controller.Chat(request, CancellationToken.None);

        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(503, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task Chat_ConversationNotFound_ReturnsNotFound()
    {
        var request = new AIChatRequestDto
        {
            PatientId = Guid.NewGuid(),
            Message = "Hello",
            ConversationId = Guid.NewGuid()
        };

        _aiServiceMock
            .Setup(x => x.ChatAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("AI conversation was not found."));

        var result = await _controller.Chat(request, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Chat_UnexpectedError_Returns500()
    {
        var request = new AIChatRequestDto
        {
            PatientId = Guid.NewGuid(),
            Message = "Hello"
        };

        _aiServiceMock
            .Setup(x => x.ChatAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        var result = await _controller.Chat(request, CancellationToken.None);

        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task SymptomCheck_ValidRequest_ReturnsOk()
    {
        var request = new AISymptomCheckRequestDto
        {
            Symptoms = "Headache, fever",
            Age = 30,
            Gender = "Male"
        };

        var response = new AISymptomCheckResponseDto
        {
            Summary = "Mild symptoms detected",
            PossibleConditions = "Common cold, Flu",
            RecommendedAction = "Rest and hydration",
            Urgency = "Low",
            RequiresEmergencyCare = false
        };

        _aiServiceMock
            .Setup(x => x.SymptomCheckAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.SymptomCheck(request, CancellationToken.None);

        _aiServiceMock.Verify(x => x.SymptomCheckAsync(
            It.Is<AISymptomCheckRequestDto>(r => r.PatientId == _authenticatedPatient.Id),
            It.IsAny<CancellationToken>()));

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var symptomResponse = Assert.IsType<AISymptomCheckResponseDto>(okResult.Value);
        Assert.NotNull(symptomResponse);
        Assert.Equal("Mild symptoms detected", symptomResponse.Summary);
    }

    [Fact]
    public async Task HealthCheck_Healthy_ReturnsOk()
    {
        _aiServiceMock
            .Setup(x => x.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Health(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task HealthCheck_Unhealthy_Returns503()
    {
        _aiServiceMock
            .Setup(x => x.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Health(CancellationToken.None);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetConversationsByPatient_ExistingPatient_ReturnsOk()
    {
        var patientId = _authenticatedPatient.Id;
        var conversations = new List<ConversationResponse>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Chat about symptoms",
                Status = "Active",
                StartedAt = DateTime.UtcNow,
                MessageCount = 5,
                EndedAt = null
            }
        };

        _aiServiceMock
            .Setup(x => x.GetConversationsByPatientAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversations);

        var result = await _controller.GetConversationsByPatient(patientId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<ConversationResponse>>>(okResult.Value);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task GetConversation_ExistingConversation_ReturnsOk()
    {
        var conversationId = Guid.NewGuid();
        var conversation = new ConversationDetailResponse
        {
            Id = conversationId,
            PatientId = _authenticatedPatient.Id,
            Title = "Test Conversation",
            Status = "Active",
            StartedAt = DateTime.UtcNow,
            Messages = new List<ConversationMessageResponse>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Role = "user",
                    Content = "Hello",
                    Timestamp = DateTime.UtcNow
                }
            }
        };

        _aiServiceMock
            .Setup(x => x.GetConversationByIdAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var result = await _controller.GetConversation(conversationId, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<ConversationDetailResponse>>(okResult.Value);
        Assert.Equal("Test Conversation", response.Data.Title);
    }

    [Fact]
    public async Task CreateConversation_ValidRequest_ReturnsCreated()
    {
        var request = new CreateConversationRequest
        {
            Title = "New Chat"
        };

        var response = new ConversationResponse
        {
            Id = Guid.NewGuid(),
            Title = "New Chat",
            Status = "Active",
            StartedAt = DateTime.UtcNow,
            MessageCount = 0,
            EndedAt = null
        };

        _aiServiceMock
            .Setup(x => x.CreateConversationAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.CreateConversation(request, CancellationToken.None);

        _aiServiceMock.Verify(x => x.CreateConversationAsync(
            It.Is<CreateConversationRequest>(r => r.PatientId == _authenticatedPatient.Id),
            It.IsAny<CancellationToken>()));

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<ConversationResponse>>(createdResult.Value);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal("New Chat", apiResponse.Data.Title);
    }
}
