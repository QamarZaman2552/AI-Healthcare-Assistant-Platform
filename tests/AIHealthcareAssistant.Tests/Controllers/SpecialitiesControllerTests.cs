using AIHealthcareAssistant.API.Controllers;
using AIHealthcareAssistant.Application.Common.Response;
using AIHealthcareAssistant.Application.Features.Specialties;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AIHealthcareAssistant.Tests.Controllers;

public class SpecialitiesControllerTests
{
    private readonly Mock<ISpecialtyService> _specialtyServiceMock;
    private readonly SpecialitiesController _controller;

    public SpecialitiesControllerTests()
    {
        _specialtyServiceMock = new Mock<ISpecialtyService>();
        _controller = new SpecialitiesController(_specialtyServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        var specialties = new List<SpecialtyResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "Cardiology", Description = "Heart specialist" }
        };

        _specialtyServiceMock
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(specialties);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiResponse<List<SpecialtyResponse>>>(okResult.Value);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task GetById_ExistingSpecialty_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var specialty = new SpecialtyResponse
        {
            Id = id,
            Name = "Cardiology",
            Description = "Heart specialist"
        };

        _specialtyServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(specialty);

        var result = await _controller.GetById(id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<ApiResponse<SpecialtyResponse>>(okResult.Value);
    }

    [Fact]
    public async Task GetById_NonExistingSpecialty_ReturnsNotFound()
    {
        var id = Guid.NewGuid();

        _specialtyServiceMock
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((SpecialtyResponse?)null);

        var result = await _controller.GetById(id);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ExistingSpecialty_ReturnsNoContent()
    {
        var id = Guid.NewGuid();

        _specialtyServiceMock
            .Setup(x => x.DeleteAsync(id))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(id);

        Assert.IsType<NoContentResult>(result);
    }
}
