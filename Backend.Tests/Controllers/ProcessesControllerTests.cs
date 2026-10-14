using Backend.Controllers;
using Backend.Models.DTOs.Requests;
using Backend.Models.DTOs.Responses;
using Backend.Models.DTOs.Responses.Common;
using Backend.Models.DTOs.Responses.Pagination;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Backend.Tests.Controllers;

public class ProcessesControllerTests
{
    private readonly Mock<IProcessService> _service = new();
    private readonly ProcessesController _controller;

    public ProcessesControllerTests()
    {
        _controller = new ProcessesController(_service.Object);
    }

    [Fact]
    public async Task Search_WhenServiceSucceeds_ReturnsOk()
    {
        var page = new PaginationResponse<ProcessResponse>
        {
            Items = [new ProcessResponse { ProcessId = "CO1.REQ.1", ProcedureName = "Obra" }],
            PageNumber = 1,
            PageSize = 20,
            TotalRecords = 1,
            TotalPages = 1
        };
        _service.Setup(service => service.SearchAsync(It.IsAny<ProcessSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginationResponse<ProcessResponse>>.Ok(page));

        var result = await _controller.Search(new ProcessSearchRequest(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(page, ok.Value);
    }

    [Fact]
    public async Task Search_WhenValidationFails_ReturnsBadRequest()
    {
        _service.Setup(service => service.SearchAsync(It.IsAny<ProcessSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PaginationResponse<ProcessResponse>>.Fail("Fecha inválida", 400));

        var result = await _controller.Search(new ProcessSearchRequest(), CancellationToken.None);

        var badRequest = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenProcessIsMissing_ReturnsNotFound()
    {
        _service.Setup(service => service.GetByIdAsync("CO1.REQ.9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProcessResponse>.Fail("No se encontró el proceso publicado.", 404));

        var result = await _controller.GetById("CO1.REQ.9", CancellationToken.None);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }
}
