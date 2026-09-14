using AIHealthcareAssistant.API.Controllers;
using AIHealthcareAssistant.Application.Common.Models;
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Availability;
using AIHealthcareAssistant.Application.Features.Doctors;
using AIHealthcareAssistant.Application.Features.Specialties;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AIHealthcareAssistant.Tests.Controllers;

public class AvailabilityControllerTests
{
    private readonly Mock<IAvailabilityService> _availabilityServiceMock;
    private readonly Mock<IDoctorService> _doctorServiceMock;
    private readonly AvailabilityController _controller;

    public AvailabilityControllerTests()
    {
        _availabilityServiceMock = new Mock<IAvailabilityService>();
        _doctorServiceMock = new Mock<IDoctorService>();
        _controller = new AvailabilityController(
            _availabilityServiceMock.Object,
            _doctorServiceMock.Object);
    }

    [Fact]
    public async Task GetByDoctor_ExistingDoctor_ReturnsOk()
    {
        var doctorId = Guid.NewGuid();
        var availabilityList = new List<AvailabilityResponse>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                DoctorName = "Dr. Smith",
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                SlotDurationMinutes = 30,
                IsActive = true
            }
        };

        _availabilityServiceMock
            .Setup(x => x.GetByDoctorAsync(doctorId))
            .ReturnsAsync(availabilityList);

        var result = await _controller.GetByDoctor(doctorId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<AvailabilityResponse>>>(okResult.Value);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task GetByDoctor_NoAvailability_ReturnsEmptyList()
    {
        var doctorId = Guid.NewGuid();

        _availabilityServiceMock
            .Setup(x => x.GetByDoctorAsync(doctorId))
            .ReturnsAsync(new List<AvailabilityResponse>());

        var result = await _controller.GetByDoctor(doctorId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<AvailabilityResponse>>>(okResult.Value);
        Assert.Empty(response.Data);
    }

    [Fact]
    public async Task GetById_ExistingAvailability_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var availability = new AvailabilityResponse
        {
            Id = id,
            DoctorId = Guid.NewGuid(),
            DoctorName = "Dr. Smith",
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            SlotDurationMinutes = 30,
            IsActive = true
        };

        _availabilityServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(availability);

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<ApiResponse<AvailabilityResponse>>(okResult.Value);
    }

    [Fact]
    public async Task GetById_NonExistingAvailability_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        _availabilityServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((AvailabilityResponse?)null);

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        var request = new CreateAvailabilityRequest
        {
            DoctorId = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            SlotDurationMinutes = 30
        };

        var response = new AvailabilityResponse
        {
            Id = Guid.NewGuid(),
            DoctorId = request.DoctorId,
            DoctorName = "Dr. Smith",
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            SlotDurationMinutes = request.SlotDurationMinutes,
            IsActive = true
        };

        _availabilityServiceMock
            .Setup(x => x.CreateAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.IsType<ApiResponse<AvailabilityResponse>>(createdResult.Value);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingAvailability_ReturnsNoContent()
    {
        var id = Guid.NewGuid();

        _availabilityServiceMock
            .Setup(x => x.DeleteAsync(id))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task SearchAvailableDoctors_ReturnsAvailableDoctors()
    {
        var specialtyId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var doctorId = Guid.NewGuid();

        var doctorResponse = new DoctorResponse
        {
            Id = doctorId,
            UserId = Guid.NewGuid(),
            FullName = "Dr. Smith",
            Email = "dr.smith@test.com",
            LicenseNumber = "LIC123",
            YearsOfExperience = 10,
            ConsultationFee = 100m,
            ClinicName = "Test Clinic",
            ClinicAddress = "123 Main St",
            IsActive = true,
            IsVerified = true,
            Specialties = new List<SpecialtyResponse> { new() { Id = specialtyId, Name = "Cardiology" } },
            CreatedAt = DateTime.UtcNow
        };

        _doctorServiceMock
            .Setup(x => x.GetDoctorsAsync(It.IsAny<DoctorQuery>()))
            .ReturnsAsync(new PagedResult<DoctorResponse>
            {
                Items = new List<DoctorResponse> { doctorResponse },
                TotalCount = 1,
                Page = 1,
                PageSize = 50
            });

        _availabilityServiceMock
            .Setup(x => x.GetAvailableSlotsAsync(doctorId, date))
            .ReturnsAsync(new List<TimeSlotResponse>
            {
                new() { StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(9, 30), IsAvailable = true }
            });

        var result = await _controller.SearchAvailableDoctors(specialtyId, date);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<DoctorResponse>>>(okResult.Value);
        Assert.Single(response.Data);
    }
}
