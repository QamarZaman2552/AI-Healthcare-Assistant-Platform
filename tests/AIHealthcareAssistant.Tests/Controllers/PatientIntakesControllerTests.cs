using AIHealthcareAssistant.API.Controllers;
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.PatientIntakes;
using AIHealthcareAssistant.Application.Features.Patients;
using AIHealthcareAssistant.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace AIHealthcareAssistant.Tests.Controllers;

public class PatientIntakesControllerTests
{
    private readonly Mock<IPatientIntakeService> _intakeServiceMock;
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly PatientIntakesController _controller;
    private readonly Guid _userId;
    private readonly Guid _patientId;
    private readonly Guid _otherPatientId;

    public PatientIntakesControllerTests()
    {
        _intakeServiceMock = new Mock<IPatientIntakeService>();
        _patientServiceMock = new Mock<IPatientService>();
        _controller = new PatientIntakesController(
            _intakeServiceMock.Object,
            _patientServiceMock.Object);
        _userId = Guid.NewGuid();
        _patientId = Guid.NewGuid();
        _otherPatientId = Guid.NewGuid();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
                    new Claim(ClaimTypes.Role, "Patient")
                }, "TestAuth"))
            }
        };

        _patientServiceMock
            .Setup(x => x.GetByUserIdAsync(_userId))
            .ReturnsAsync(new PatientResponse
            {
                Id = _patientId,
                UserId = _userId,
                FullName = "John Doe",
                Email = "john@test.com",
                CreatedAt = DateTime.UtcNow
            });
    }

    private PatientIntakeResponse OwnIntake() => new()
    {
        Id = Guid.NewGuid(),
        PatientId = _patientId,
        PatientName = "John Doe",
        ChiefComplaint = "Headache",
        Status = IntakeStatus.Submitted,
        SubmittedAt = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow
    };

    private PatientIntakeResponse OtherIntake() => new()
    {
        Id = Guid.NewGuid(),
        PatientId = _otherPatientId,
        PatientName = "Jane Doe",
        ChiefComplaint = "Fever",
        Status = IntakeStatus.Submitted,
        SubmittedAt = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetById_OwnIntake_ReturnsOk()
    {
        var intake = OwnIntake();

        _intakeServiceMock
            .Setup(x => x.GetByIdAsync(intake.Id))
            .ReturnsAsync(intake);

        var result = await _controller.GetById(intake.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PatientIntakeResponse>>(okResult.Value);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task GetById_OtherPatientsIntake_ReturnsNotFound()
    {
        var intake = OtherIntake();

        _intakeServiceMock
            .Setup(x => x.GetByIdAsync(intake.Id))
            .ReturnsAsync(intake);

        var result = await _controller.GetById(intake.Id);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_NonExisting_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        _intakeServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((PatientIntakeResponse?)null);

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByPatient_Own_ReturnsOk()
    {
        _intakeServiceMock
            .Setup(x => x.GetByPatientAsync(_patientId))
            .ReturnsAsync(new List<PatientIntakeResponse> { OwnIntake() });

        var result = await _controller.GetByPatient(_patientId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<PatientIntakeResponse>>>(okResult.Value);
        Assert.NotNull(response.Data);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task GetByPatient_OtherPatient_ReturnsNotFound()
    {
        var result = await _controller.GetByPatient(_otherPatientId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByAppointment_OwnIntake_ReturnsOk()
    {
        var appointmentId = Guid.NewGuid();
        var intake = OwnIntake();

        _intakeServiceMock
            .Setup(x => x.GetByAppointmentAsync(appointmentId))
            .ReturnsAsync(intake);

        var result = await _controller.GetByAppointment(appointmentId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PatientIntakeResponse>>(okResult.Value);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task GetByAppointment_OtherPatientsIntake_ReturnsNotFound()
    {
        var appointmentId = Guid.NewGuid();
        var intake = OtherIntake();

        _intakeServiceMock
            .Setup(x => x.GetByAppointmentAsync(appointmentId))
            .ReturnsAsync(intake);

        var result = await _controller.GetByAppointment(appointmentId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByAppointment_NonExisting_ReturnsNotFound()
    {
        var appointmentId = Guid.NewGuid();

        _intakeServiceMock
            .Setup(x => x.GetByAppointmentAsync(appointmentId))
            .ReturnsAsync((PatientIntakeResponse?)null);

        var result = await _controller.GetByAppointment(appointmentId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        SetRole("Doctor");

        var intake = OwnIntake();
        var request = new CreatePatientIntakeRequest
        {
            PatientId = _patientId,
            ChiefComplaint = "Persistent headache for 3 days"
        };

        _intakeServiceMock
            .Setup(x => x.CreateAsync(request))
            .ReturnsAsync(intake);

        var result = await _controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PatientIntakeResponse>>(createdResult.Value);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ValidRequest_ReturnsOk()
    {
        SetRole("Doctor");

        var intake = OwnIntake();
        var request = new UpdatePatientIntakeStatusRequest
        {
            Status = IntakeStatus.ReviewedByDoctor
        };

        _intakeServiceMock
            .Setup(x => x.UpdateStatusAsync(intake.Id, IntakeStatus.ReviewedByDoctor))
            .ReturnsAsync(intake);

        var result = await _controller.UpdateStatus(intake.Id, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PatientIntakeResponse>>(okResult.Value);
        Assert.NotNull(response.Data);
    }

    private void SetRole(string role)
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
                    new Claim(ClaimTypes.Role, role)
                }, "TestAuth"))
            }
        };
    }
}
