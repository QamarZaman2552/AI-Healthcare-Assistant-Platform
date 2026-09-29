using AIHealthcareAssistant.API.Controllers;
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Patients;
using AIHealthcareAssistant.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace AIHealthcareAssistant.Tests.Controllers;

public class PatientsControllerTests
{
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly PatientsController _controller;
    private readonly Guid _userId;
    private readonly Guid _patientId;

    public PatientsControllerTests()
    {
        _patientServiceMock = new Mock<IPatientService>();
        _controller = new PatientsController(_patientServiceMock.Object);
        _userId = Guid.NewGuid();
        _patientId = Guid.NewGuid();

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
        Assert.NotNull(response.Data);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task GetById_ExistingPatient_ReturnsOk()
    {
        var patient = new PatientResponse
        {
            Id = _patientId,
            UserId = _userId,
            FullName = "John Doe",
            Email = "john@test.com",
            CreatedAt = DateTime.UtcNow
        };

        _patientServiceMock
            .Setup(x => x.GetByIdAsync(_patientId))
            .ReturnsAsync(patient);

        var result = await _controller.GetById(_patientId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PatientResponse>>(okResult.Value);
        Assert.NotNull(response.Data);
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
    public async Task GetById_OtherPatient_ReturnsNotFound()
    {
        var otherPatientId = Guid.NewGuid();
        var otherPatient = new PatientResponse
        {
            Id = otherPatientId,
            UserId = Guid.NewGuid(),
            FullName = "Jane Doe",
            Email = "jane@test.com",
            CreatedAt = DateTime.UtcNow
        };

        _patientServiceMock
            .Setup(x => x.GetByIdAsync(otherPatientId))
            .ReturnsAsync(otherPatient);

        var result = await _controller.GetById(otherPatientId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByUserId_ExistingPatient_ReturnsOk()
    {
        var patient = new PatientResponse
        {
            Id = _patientId,
            UserId = _userId,
            FullName = "John Doe",
            Email = "john@test.com",
            CreatedAt = DateTime.UtcNow
        };

        _patientServiceMock
            .Setup(x => x.GetByUserIdAsync(_userId))
            .ReturnsAsync(patient);

        var result = await _controller.GetByUserId(_userId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PatientResponse>>(okResult.Value);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task GetByUserId_OtherPatient_ReturnsNotFound()
    {
        var otherUserId = Guid.NewGuid();
        var otherPatient = new PatientResponse
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            FullName = "Jane Doe",
            Email = "jane@test.com",
            CreatedAt = DateTime.UtcNow
        };

        _patientServiceMock
            .Setup(x => x.GetByUserIdAsync(otherUserId))
            .ReturnsAsync(otherPatient);

        var result = await _controller.GetByUserId(otherUserId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByUserId_NonExistingPatient_ReturnsNotFound()
    {
        var unknownUserId = Guid.NewGuid();

        _patientServiceMock
            .Setup(x => x.GetByUserIdAsync(unknownUserId))
            .ReturnsAsync((PatientResponse?)null);

        var result = await _controller.GetByUserId(unknownUserId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetProfile_Existing_ReturnsOk()
    {
        var profile = new PatientProfileResponse
        {
            DateOfBirth = new DateOnly(1990, 5, 14),
            Gender = Gender.Male,
            City = "Lahore"
        };

        _patientServiceMock
            .Setup(x => x.GetProfileAsync(_patientId))
            .ReturnsAsync(profile);

        var result = await _controller.GetProfile(_patientId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PatientProfileResponse>>(okResult.Value);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task GetProfile_NoProfile_ReturnsNotFound()
    {
        _patientServiceMock
            .Setup(x => x.GetProfileAsync(_patientId))
            .ReturnsAsync((PatientProfileResponse?)null);

        var result = await _controller.GetProfile(_patientId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetProfile_OtherPatient_ReturnsNotFound()
    {
        var otherPatientId = Guid.NewGuid();
        var profile = new PatientProfileResponse
        {
            DateOfBirth = new DateOnly(1992, 1, 1),
            Gender = Gender.Female
        };

        _patientServiceMock
            .Setup(x => x.GetProfileAsync(otherPatientId))
            .ReturnsAsync(profile);

        var result = await _controller.GetProfile(otherPatientId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetHistory_ExistingPatient_ReturnsOk()
    {
        var history = new PatientHistoryResponse
        {
            PatientId = _patientId,
            FullName = "John Doe",
            Email = "john@test.com",
            Appointments = new List<Application.Features.Appointments.AppointmentResponse>(),
            TotalAppointments = 0,
            CompletedAppointments = 0,
            CancelledAppointments = 0
        };

        _patientServiceMock
            .Setup(x => x.GetHistoryAsync(_patientId))
            .ReturnsAsync(history);

        var result = await _controller.GetHistory(_patientId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PatientHistoryResponse>>(okResult.Value);
        Assert.NotNull(response.Data);
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
    public async Task GetHistory_OtherPatient_ReturnsNotFound()
    {
        var otherPatientId = Guid.NewGuid();
        var history = new PatientHistoryResponse
        {
            PatientId = otherPatientId,
            FullName = "Jane Doe",
            Email = "jane@test.com",
            Appointments = new List<Application.Features.Appointments.AppointmentResponse>(),
            TotalAppointments = 0,
            CompletedAppointments = 0,
            CancelledAppointments = 0
        };

        _patientServiceMock
            .Setup(x => x.GetHistoryAsync(otherPatientId))
            .ReturnsAsync(history);

        var result = await _controller.GetHistory(otherPatientId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsCreated()
    {
        var patient = new PatientResponse
        {
            Id = _patientId,
            UserId = _userId,
            FullName = "John Doe",
            Email = "john@test.com"
        };

        _patientServiceMock
            .Setup(x => x.RegisterAsync(_userId))
            .ReturnsAsync(patient);

        var result = await _controller.Register();

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PatientResponse>>(createdResult.Value);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task GetDashboardStats_ExistingPatient_ReturnsOk()
    {
        var dashboard = new PatientDashboardResponse
        {
            TotalAppointments = 5,
            UpcomingAppointments = 2,
            RecentActivity = new List<RecentActivityResponse>
            {
                new()
                {
                    AppointmentId = Guid.NewGuid(),
                    DoctorName = "Dr. Smith",
                    Status = "Confirmed",
                    ScheduledStart = DateTime.UtcNow.AddDays(1),
                    Type = "Appointment"
                }
            }
        };

        _patientServiceMock
            .Setup(x => x.GetDashboardStatsAsync(_patientId))
            .ReturnsAsync(dashboard);

        var result = await _controller.GetDashboardStats(_patientId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<PatientDashboardResponse>>(okResult.Value);
        Assert.NotNull(response.Data);
        Assert.Equal(5, response.Data.TotalAppointments);
        Assert.Equal(2, response.Data.UpcomingAppointments);
    }

    [Fact]
    public async Task GetDashboardStats_NonExistingPatient_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        _patientServiceMock
            .Setup(x => x.GetDashboardStatsAsync(id))
            .ReturnsAsync((PatientDashboardResponse?)null);

        var result = await _controller.GetDashboardStats(id);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetDashboardStats_OtherPatient_ReturnsNotFound()
    {
        var otherPatientId = Guid.NewGuid();

        var result = await _controller.GetDashboardStats(otherPatientId);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
