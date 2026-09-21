using AIHealthcareAssistant.API.Controllers;
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Doctors;
using AIHealthcareAssistant.Application.Features.Appointments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace AIHealthcareAssistant.Tests.Controllers;

public class DoctorControllerTests
{
    private readonly Mock<IDoctorService> _doctorServiceMock;
    private readonly Mock<ILogger<DoctorsController>> _loggerMock;
    private readonly DoctorsController _controller;

    public DoctorControllerTests()
    {
        _doctorServiceMock = new Mock<IDoctorService>();
        _loggerMock = new Mock<ILogger<DoctorsController>>();
        _controller = new DoctorsController(_doctorServiceMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Doctor")
                }))
            }
        };
    }

    [Fact]
    public async Task GetDashboardStats_ExistingDoctor_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var dashboard = new DoctorDashboardResponse
        {
            TotalAppointments = 10,
            TodayAppointments = 2,
            UpcomingAppointments = 5,
            CompletedAppointments = 3,
            RecentAppointments = new List<AppointmentSummaryResponse>()
        };

        _doctorServiceMock
            .Setup(x => x.GetDashboardStatsAsync(id))
            .ReturnsAsync(dashboard);

        var result = await _controller.GetDashboardStats(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DoctorDashboardResponse>>(okResult.Value);
        Assert.Equal(10, response.Data.TotalAppointments);
    }

    [Fact]
    public async Task GetDashboardStats_NonExistingDoctor_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        _doctorServiceMock
            .Setup(x => x.GetDashboardStatsAsync(id))
            .ReturnsAsync((DoctorDashboardResponse?)null);

        var result = await _controller.GetDashboardStats(id);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAppointmentsByDoctor_ExistingDoctor_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var appointments = new List<DoctorAppointmentResponse>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PatientName = "John Doe",
                ScheduledStart = DateTime.UtcNow.AddHours(1),
                Status = "Confirmed",
                DurationMinutes = 30
            }
        };

        _doctorServiceMock
            .Setup(x => x.GetDoctorAppointmentsAsync(id, It.IsAny<DateOnly>()))
            .ReturnsAsync(appointments);

        var result = await _controller.GetAppointmentsByDoctor(id, "today");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<DoctorAppointmentResponse>>>(okResult.Value);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task UpdateProfile_ExistingDoctor_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var doctor = new DoctorResponse
        {
            Id = id,
            UserId = Guid.NewGuid(),
            FullName = "Dr. Smith",
            Email = "doctor@test.com"
        };

        var updatedDoctor = new DoctorResponse
        {
            Id = id,
            UserId = doctor.UserId,
            FullName = "Dr. Smith Updated",
            Email = "doctor@test.com"
        };

        _doctorServiceMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(doctor);

        _doctorServiceMock
            .Setup(x => x.UpdateAsync(id, It.IsAny<UpdateDoctorRequest>()))
            .ReturnsAsync(updatedDoctor);

        var request = new UpdateDoctorRequest
        {
            LicenseNumber = "NEW-LICENSE",
            YearsOfExperience = 10
        };

        var result = await _controller.UpdateProfile(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DoctorResponse>>(okResult.Value);
        Assert.Equal("Dr. Smith Updated", response.Data.FullName);
    }

    [Fact]
    public async Task UpdateProfile_NonExistingDoctor_ReturnsNotFound()
    {
        _doctorServiceMock
            .Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((DoctorResponse?)null);

        var request = new UpdateDoctorRequest
        {
            LicenseNumber = "NEW-LICENSE"
        };

        var result = await _controller.UpdateProfile(request);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
