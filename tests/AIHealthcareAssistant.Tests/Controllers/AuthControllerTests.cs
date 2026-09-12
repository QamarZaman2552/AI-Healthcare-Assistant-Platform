using AIHealthcareAssistant.API.Controllers;
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Auth;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AIHealthcareAssistant.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        var request = new RegisterRequest
        {
            Email = "test@test.com",
            Password = "Test@1234",
            FirstName = "John",
            LastName = "Doe"
        };

        var response = new AuthResponse
        {
            Token = "jwt-token",
            Email = request.Email,
            FullName = "John Doe",
            Role = "Patient",
            Expiration = DateTime.UtcNow.AddHours(1)
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.Register(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var authResponse = Assert.IsType<ApiResponse<AuthResponse>>(okResult.Value);
        Assert.Equal("jwt-token", authResponse.Data.Token);
        Assert.Equal("test@test.com", authResponse.Data.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsError()
    {
        var request = new RegisterRequest
        {
            Email = "existing@test.com",
            Password = "Test@1234",
            FirstName = "John",
            LastName = "Doe"
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(request))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Register(request));

        Assert.Equal("Email already exists", ex.Message);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "Test@1234"
        };

        var response = new AuthResponse
        {
            Token = "jwt-token",
            Email = request.Email,
            FullName = "John Doe",
            Role = "Patient",
            Expiration = DateTime.UtcNow.AddHours(1)
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var authResponse = Assert.IsType<ApiResponse<AuthResponse>>(okResult.Value);
        Assert.Equal("jwt-token", authResponse.Data.Token);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsError()
    {
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "WrongPassword"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.Login(request));

        Assert.Equal("Invalid credentials", ex.Message);
    }
}
