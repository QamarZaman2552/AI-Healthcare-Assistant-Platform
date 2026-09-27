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
    private readonly Guid _userId;

    public DoctorControllerTests()
    {
        _doctorServiceMock = new Mock<IDoctorService>();
        _loggerMock = new Mock<ILogger<DoctorsController>>();
        _controller = new DoctorsController(_doctorServiceMock.Object);
        _userId = Guid.NewGuid();

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
                    new Claim(ClaimTypes.Role, "Doctor")
                }, "TestAuth"))
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
            .Setup(x => x.GetByUserIdAsync(_userId))
            .ReturnsAsync(new DoctorResponse { Id = id, UserId = _userId });

        _doctorServiceMock
            .Setup(x => x.GetDashboardStatsAsync(id))
            .ReturnsAsync(dashboard);

        var result = await _controller.GetDashboardStats(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<DoctorDashboardResponse>>(okResult.Value);
        Assert.NotNull(response.Data);
        Assert.Equal(10, response.Data.TotalAppointments);
    }

    [Fact]
    public async Task GetDashboardStats_NonExistingDoctor_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        _doctorServiceMock
            .Setup(x => x.GetByUserIdAsync(_userId))
            .ReturnsAsync(new DoctorResponse { Id = id, UserId = _userId });

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
            .Setup(x => x.GetByUserIdAsync(_userId))
            .ReturnsAsync(new DoctorResponse { Id = id, UserId = _userId });

        _doctorServiceMock
            .Setup(x => x.GetDoctorAppointmentsAsync(id, It.IsAny<DateOnly>()))
            .ReturnsAsync(appointments);

        var result = await _controller.GetAppointmentsByDoctor(id, "today");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<DoctorAppointmentResponse>>>(okResult.Value);
        Assert.NotNull(response.Data);
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
        Assert.NotNull(response.Data);
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

    private void SetupOwnDoctor(Guid ownDoctorId)
    {
        _doctorServiceMock
            .Setup(x => x.GetByUserIdAsync(_userId))
            .ReturnsAsync(new DoctorResponse { Id = ownDoctorId, UserId = _userId });
    }

    [Fact]
    public async Task Update_OtherDoctorsRecord_ReturnsForbidden()
    {
        SetupOwnDoctor(Guid.NewGuid());
        var otherDoctorId = Guid.NewGuid();

        var result = await _controller.Update(otherDoctorId, new UpdateDoctorRequest { LicenseNumber = "LIC" });

        var objResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objResult.StatusCode);
        _doctorServiceMock.Verify(
            x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateDoctorRequest>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_OwnDoctor_ReturnsOk()
    {
        var ownDoctorId = Guid.NewGuid();
        SetupOwnDoctor(ownDoctorId);
        _doctorServiceMock
            .Setup(x => x.UpdateAsync(ownDoctorId, It.IsAny<UpdateDoctorRequest>()))
            .ReturnsAsync(new DoctorResponse { Id = ownDoctorId, UserId = _userId });

        var result = await _controller.Update(ownDoctorId, new UpdateDoctorRequest { LicenseNumber = "LIC" });

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_OtherDoctorsRecord_ReturnsForbidden()
    {
        SetupOwnDoctor(Guid.NewGuid());
        var otherDoctorId = Guid.NewGuid();

        var result = await _controller.UpdateStatus(otherDoctorId, new UpdateDoctorStatusRequest { IsActive = true });

        var objResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objResult.StatusCode);
        _doctorServiceMock.Verify(
            x => x.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateStatus_OwnDoctor_ReturnsOk()
    {
        var ownDoctorId = Guid.NewGuid();
        SetupOwnDoctor(ownDoctorId);
        _doctorServiceMock
            .Setup(x => x.UpdateStatusAsync(ownDoctorId, true))
            .ReturnsAsync(new DoctorResponse { Id = ownDoctorId, UserId = _userId });

        var result = await _controller.UpdateStatus(ownDoctorId, new UpdateDoctorStatusRequest { IsActive = true });

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateStatus_MissingIsActive_ReturnsBadRequest()
    {
        var ownDoctorId = Guid.NewGuid();
        SetupOwnDoctor(ownDoctorId);

        var result = await _controller.UpdateStatus(ownDoctorId, new UpdateDoctorStatusRequest { IsActive = null });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _doctorServiceMock.Verify(
            x => x.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateVerification_MissingIsVerified_ReturnsBadRequest()
    {
        var result = await _controller.UpdateVerification(Guid.NewGuid(), new UpdateDoctorVerificationRequest { IsVerified = null });

        Assert.IsType<BadRequestObjectResult>(result.Result);
        _doctorServiceMock.Verify(
            x => x.UpdateVerificationAsync(It.IsAny<Guid>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignSpecialty_OtherDoctorsRecord_ReturnsForbidden()
    {
        SetupOwnDoctor(Guid.NewGuid());
        var otherDoctorId = Guid.NewGuid();

        var result = await _controller.AssignSpecialty(
            otherDoctorId,
            new AssignSpecialtyRequest { SpecialtyId = Guid.NewGuid() });

        var objResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(403, objResult.StatusCode);
        _doctorServiceMock.Verify(
            x => x.AssignSpecialtyAsync(It.IsAny<Guid>(), It.IsAny<AssignSpecialtyRequest>()),
            Times.Never);
    }

    [Fact]
    public async Task RemoveSpecialty_OtherDoctorsRecord_ReturnsForbidden()
    {
        SetupOwnDoctor(Guid.NewGuid());
        var otherDoctorId = Guid.NewGuid();

        var result = await _controller.RemoveSpecialty(otherDoctorId, Guid.NewGuid());

        var objResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objResult.StatusCode);
        _doctorServiceMock.Verify(
            x => x.RemoveSpecialtyAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task GetDashboardStats_MissingId_ReturnsBadRequest()
    {
        var result = await _controller.GetDashboardStats(Guid.Empty);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
