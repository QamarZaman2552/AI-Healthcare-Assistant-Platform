using AIHealthcareAssistant.API.Controllers;
using AIHealthcareAssistant.Application.Common.Interfaces;
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

    public AiControllerTests()
    {
        _aiServiceMock = new Mock<IAIService>();
        _patientServiceMock = new Mock<IPatientService>();
        _loggerMock = new Mock<ILogger<AiController>>();
        _controller = new AiController(_aiServiceMock.Object, _patientServiceMock.Object, _loggerMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                }))
            }
        };

        _patientServiceMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new PatientResponse
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FullName = "Test Patient",
                Email = "test@test.com"
            });
    }

    [Fact]
    public async Task Chat_ValidRequest_ReturnsOk()
    {
        var request = new AIChatRequestDto
        {
            PatientId = Guid.NewGuid(),
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

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var chatResponse = Assert.IsType<AIChatResponseDto>(okResult.Value);
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
            PatientId = Guid.NewGuid(),
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

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<AISymptomCheckResponseDto>(okResult.Value);
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
}
