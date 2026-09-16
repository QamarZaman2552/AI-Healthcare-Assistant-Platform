using AIHealthcareAssistant.API.Controllers;
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Patients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace AIHealthcareAssistant.Tests.Controllers;

public class PatientsControllerTests
{
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly PatientsController _controller;

    public PatientsControllerTests()
    {
        _patientServiceMock = new Mock<IPatientService>();
        _controller = new PatientsController(_patientServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var patients = new List<PatientResponse>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john@test.com",
                CreatedAt = DateTime.UtcNow
            }
        };

        _patientServiceMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(patients);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<PatientResponse>>>(okResult.Value);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task GetById_ExistingPatient_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var patient = new PatientResponse
        {
            Id = id,
            UserId = Guid.NewGuid(),
            FullName = "John Doe",
            Email = "john@test.com",
            CreatedAt = DateTime.UtcNow
        };

        _patientServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(patient);

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<ApiResponse<PatientResponse>>(okResult.Value);
    }

    [Fact]
    public async Task GetById_NonExistingPatient_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        _patientServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((PatientResponse?)null);

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetHistory_ExistingPatient_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var history = new PatientHistoryResponse
        {
            PatientId = id,
            FullName = "John Doe",
            Email = "john@test.com",
            Appointments = new List<Application.Features.Appointments.AppointmentResponse>(),
            TotalAppointments = 0,
            CompletedAppointments = 0,
            CancelledAppointments = 0
        };

        _patientServiceMock
            .Setup(x => x.GetHistoryAsync(id))
            .ReturnsAsync(history);

        var result = await _controller.GetHistory(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<ApiResponse<PatientHistoryResponse>>(okResult.Value);
    }

    [Fact]
    public async Task GetHistory_NonExistingPatient_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        _patientServiceMock
            .Setup(x => x.GetHistoryAsync(id))
            .ReturnsAsync((PatientHistoryResponse?)null);

        var result = await _controller.GetHistory(id);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsCreated()
    {
        var userId = Guid.NewGuid();
        var patient = new PatientResponse
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = "John Doe",
            Email = "john@test.com"
        };

        _patientServiceMock
            .Setup(x => x.RegisterAsync(userId))
            .ReturnsAsync(patient);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }))
            }
        };

        var result = await _controller.Register();

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PatientResponse>>(createdResult.Value);
        Assert.Equal(201, createdResult.StatusCode);
    }
}
