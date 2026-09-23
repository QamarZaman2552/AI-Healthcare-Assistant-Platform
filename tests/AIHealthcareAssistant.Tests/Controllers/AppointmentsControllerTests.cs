using AIHealthcareAssistant.API.Controllers;
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Appointments;
using AIHealthcareAssistant.Application.Features.Doctors;
using AIHealthcareAssistant.Application.Features.Patients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace AIHealthcareAssistant.Tests.Controllers;

public class AppointmentsControllerTests
{
    private readonly Mock<IAppointmentService> _appointmentServiceMock;
    private readonly Mock<IPatientService> _patientServiceMock;
    private readonly Mock<IDoctorService> _doctorServiceMock;
    private readonly AppointmentsController _controller;

    public AppointmentsControllerTests()
    {
        _appointmentServiceMock = new Mock<IAppointmentService>();
        _patientServiceMock = new Mock<IPatientService>();
        _doctorServiceMock = new Mock<IDoctorService>();
        _controller = new AppointmentsController(
            _appointmentServiceMock.Object,
            _patientServiceMock.Object,
            _doctorServiceMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.Role, "Admin")
                }, "TestAuth"))
            }
        };
    }

    [Fact]
    public async Task GetById_ExistingAppointment_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var appointment = new AppointmentResponse
        {
            Id = id,
            PatientId = Guid.NewGuid(),
            PatientName = "John Doe",
            DoctorId = Guid.NewGuid(),
            DoctorName = "Dr. Smith",
            Status = "Scheduled",
            ScheduledStart = DateTime.UtcNow.AddDays(1),
            ScheduledEnd = DateTime.UtcNow.AddDays(1).AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        _appointmentServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(appointment);

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<AppointmentResponse>>(okResult.Value);
        Assert.NotNull(response.Data);
    }

    [Fact]
    public async Task GetById_NonExistingAppointment_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        _appointmentServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((AppointmentResponse?)null);

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetByPatient_ReturnsOk()
    {
        var patientId = Guid.NewGuid();
        var appointments = new List<AppointmentResponse>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                PatientName = "John Doe",
                DoctorId = Guid.NewGuid(),
                DoctorName = "Dr. Smith",
                Status = "Scheduled",
                ScheduledStart = DateTime.UtcNow.AddDays(1),
                ScheduledEnd = DateTime.UtcNow.AddDays(1).AddHours(1),
                CreatedAt = DateTime.UtcNow
            }
        };

        _appointmentServiceMock
            .Setup(x => x.GetByPatientAsync(patientId))
            .ReturnsAsync(appointments);

        var result = await _controller.GetByPatient(patientId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<List<AppointmentResponse>>>(okResult.Value);
        Assert.NotNull(apiResponse.Data);
        Assert.Single(apiResponse.Data);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        var request = new CreateAppointmentRequest
        {
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            ScheduledStart = DateTime.UtcNow.AddDays(1),
            ScheduledEnd = DateTime.UtcNow.AddDays(1).AddHours(1),
            ReasonForVisit = "General checkup"
        };

        var response = new AppointmentResponse
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            PatientName = "John Doe",
            DoctorId = request.DoctorId,
            DoctorName = "Dr. Smith",
            Status = "Scheduled",
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledEnd,
            ReasonForVisit = request.ReasonForVisit,
            CreatedAt = DateTime.UtcNow
        };

        _appointmentServiceMock
            .Setup(x => x.CreateAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<AppointmentResponse>>(createdResult.Value);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task Cancel_ExistingAppointment_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var existing = new AppointmentResponse
        {
            Id = id,
            PatientId = Guid.NewGuid(),
            PatientName = "John Doe",
            DoctorId = Guid.NewGuid(),
            DoctorName = "Dr. Smith",
            Status = "Scheduled",
            ScheduledStart = DateTime.UtcNow.AddDays(1),
            ScheduledEnd = DateTime.UtcNow.AddDays(1).AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        var response = new AppointmentResponse
        {
            Id = id,
            PatientId = existing.PatientId,
            PatientName = "John Doe",
            DoctorId = existing.DoctorId,
            DoctorName = "Dr. Smith",
            Status = "Cancelled",
            ScheduledStart = DateTime.UtcNow.AddDays(1),
            ScheduledEnd = DateTime.UtcNow.AddDays(1).AddHours(1),
            CancellationReason = "Patient request",
            CancelledAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _appointmentServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(existing);

        _appointmentServiceMock
            .Setup(x => x.CancelAsync(id, It.IsAny<string?>()))
            .ReturnsAsync(response);

        var result = await _controller.Cancel(
            id,
            new CancelAppointmentRequest { CancellationReason = "Patient request" });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<AppointmentResponse>>(okResult.Value);
        Assert.NotNull(apiResponse.Data);
    }

    [Fact]
    public async Task Reschedule_ValidRequest_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var request = new RescheduleAppointmentRequest
        {
            NewScheduledStart = DateTime.UtcNow.AddDays(2),
            NewScheduledEnd = DateTime.UtcNow.AddDays(2).AddHours(1)
        };

        var existing = new AppointmentResponse
        {
            Id = id,
            PatientId = Guid.NewGuid(),
            PatientName = "John Doe",
            DoctorId = Guid.NewGuid(),
            DoctorName = "Dr. Smith",
            Status = "Scheduled",
            ScheduledStart = DateTime.UtcNow.AddDays(1),
            ScheduledEnd = DateTime.UtcNow.AddDays(1).AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        var response = new AppointmentResponse
        {
            Id = id,
            PatientId = existing.PatientId,
            PatientName = "John Doe",
            DoctorId = existing.DoctorId,
            DoctorName = "Dr. Smith",
            Status = "Rescheduled",
            ScheduledStart = request.NewScheduledStart,
            ScheduledEnd = request.NewScheduledEnd,
            CreatedAt = DateTime.UtcNow
        };

        _appointmentServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(existing);

        _appointmentServiceMock
            .Setup(x => x.RescheduleAsync(id, request))
            .ReturnsAsync(response);

        var result = await _controller.Reschedule(id, request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse<AppointmentResponse>>(okResult.Value);
        Assert.NotNull(apiResponse.Data);
    }

    [Fact]
    public async Task Cancel_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        var id = Guid.NewGuid();

        _appointmentServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(new AppointmentResponse { Id = id, Status = "Cancelled" });
        _appointmentServiceMock
            .Setup(x => x.CancelAsync(id, It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("Appointment is already cancelled"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Cancel(id, new CancelAppointmentRequest()));
        Assert.Contains("already cancelled", ex.Message);
    }

    [Fact]
    public async Task Reschedule_CancelledAppointment_ThrowsInvalidOperationException()
    {
        var id = Guid.NewGuid();

        _appointmentServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(new AppointmentResponse { Id = id, Status = "Cancelled" });
        _appointmentServiceMock
            .Setup(x => x.RescheduleAsync(id, It.IsAny<RescheduleAppointmentRequest>()))
            .ThrowsAsync(new InvalidOperationException("Cannot reschedule a cancelled appointment"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Reschedule(id, new RescheduleAppointmentRequest
            {
                NewScheduledStart = DateTime.UtcNow.AddDays(2),
                NewScheduledEnd = DateTime.UtcNow.AddDays(2).AddHours(1)
            }));
        Assert.Contains("cancelled appointment", ex.Message);
    }

    [Fact]
    public async Task Create_DoctorUnavailable_ThrowsInvalidOperationException()
    {
        var request = new CreateAppointmentRequest
        {
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            ScheduledStart = DateTime.UtcNow.AddDays(1),
            ScheduledEnd = DateTime.UtcNow.AddDays(1).AddHours(1),
            ReasonForVisit = "Checkup"
        };

        _appointmentServiceMock
            .Setup(x => x.CreateAsync(request))
            .ThrowsAsync(new InvalidOperationException("Doctor is not available at this time slot"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.Create(request));
        Assert.Contains("not available", ex.Message);
    }
}
