using AIHealthcareAssistant.API.Controllers;
using AIHealthcareAssistant.Application.Features.Admin;
using AIHealthcareAssistant.Application.Common.Response;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AIHealthcareAssistant.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IAdminService> _adminServiceMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _adminServiceMock = new Mock<IAdminService>();
        _controller = new AdminController(_adminServiceMock.Object);
    }

    [Fact]
    public async Task GetDashboard_ReturnsOk()
    {
        var dashboard = new AdminDashboardResponse
        {
            TotalPatients = 10,
            TotalDoctors = 5,
            TotalAppointments = 20,
            TodayAppointments = 2,
            ActiveUsers = 15,
            Status = "active",
            LastUpdated = DateTime.UtcNow
        };

        _adminServiceMock
            .Setup(x => x.GetDashboardAsync())
            .ReturnsAsync(dashboard);

        var result = await _controller.GetDashboard();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AdminDashboardResponse>>(okResult.Value);
        Assert.Equal(10, response.Data.TotalPatients);
    }

    [Fact]
    public async Task GetUsers_ReturnsOk()
    {
        var users = new List<UserResponse>
        {
            new() { Id = Guid.NewGuid(), Email = "admin@test.com", FullName = "Admin", Role = "Admin", IsActive = true }
        };

        _adminServiceMock
            .Setup(x => x.GetUsersAsync())
            .ReturnsAsync(users);

        var result = await _controller.GetUsers();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<UserResponse>>>(okResult.Value);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task GetSystemStatus_ReturnsOk()
    {
        var status = new SystemStatusResponse
        {
            Status = "operational",
            DatabaseConnected = true,
            AIAvailable = true,
            ActiveConnections = 10,
            LastChecked = DateTime.UtcNow
        };

        _adminServiceMock
            .Setup(x => x.GetSystemStatusAsync())
            .ReturnsAsync(status);

        var result = await _controller.GetSystemStatus();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<SystemStatusResponse>>(okResult.Value);
        Assert.Equal("operational", response.Data.Status);
    }
}
