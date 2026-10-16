using AutoMapper;
using Backend.Clients;
using Backend.Mappings.Profiles;
using Backend.Models.DTOs.Requests;
using Backend.Models.External;
using Backend.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Backend.Tests.Services;

public class ProcessServiceTests
{
    private readonly Mock<IConvocaApiClient> _client = new();
    private readonly ProcessService _service;

    public ProcessServiceTests()
    {
        var mapper = new MapperConfiguration(config => config.AddProfile<ProcessProfile>()).CreateMapper();
        _service = new ProcessService(_client.Object, mapper, NullLogger<ProcessService>.Instance);
    }

    [Fact]
    public async Task SearchAsync_WhenDatesAreInverted_ReturnsBadRequest()
    {
        var result = await _service.SearchAsync(new ProcessSearchRequest
        {
            PublishedFrom = new DateTime(2026, 5, 2),
            PublishedTo = new DateTime(2026, 1, 1)
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        _client.Verify(client => client.SearchAsync(
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WhenPublishedByDefault_QueriesUpstreamAndMapsRows()
    {
        string? capturedWhere = null;
        _client.Setup(client => client.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                21,
                0,
                true,
                It.IsAny<CancellationToken>()))
            .Callback<string, string?, int, int, bool, CancellationToken>((where, _, _, _, _, _) => capturedWhere = where)
            .ReturnsAsync(
            [
                new ConvocaProcessRecord
                {
                    ProcessId = "CO1.REQ.10",
                    EntityName = "Hospital",
                    ProcedureName = "Dotación",
                    BasePrice = "1500",
                    PublishedOn = "2026-10-01T00:00:00.000"
                }
            ]);

        var result = await _service.SearchAsync(new ProcessSearchRequest(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(result.Value!.HasMore);
        Assert.Equal(1, result.Value.TotalRecords);
        Assert.Equal("CO1.REQ.10", result.Value.Items[0].ProcessId);
        Assert.Equal(1500m, result.Value.Items[0].BasePrice);
        Assert.NotNull(capturedWhere);
        Assert.Contains("fecha_de_publicacion_del IS NOT NULL", capturedWhere, StringComparison.Ordinal);
        Assert.Contains("!= 'Borrador'", capturedWhere, StringComparison.Ordinal);
        _client.Verify(client => client.CountAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WhenUpstreamFails_ReturnsBadGateway()
    {
        _client.Setup(client => client.SearchAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConvocaUnavailableException("down"));

        var result = await _service.SearchAsync(new ProcessSearchRequest(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(502, result.StatusCode);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProcessDoesNotExist_ReturnsNotFound()
    {
        _client.Setup(client => client.SearchAsync(
                It.IsAny<string>(),
                null,
                1,
                0,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _service.GetByIdAsync("CO1.REQ.99", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdIsUnsafe_ReturnsBadRequest()
    {
        var result = await _service.GetByIdAsync("CO1.REQ.1' OR 1=1", CancellationToken.None);

        Assert.Equal(400, result.StatusCode);
        _client.Verify(client => client.SearchAsync(
            It.IsAny<string>(),
            It.IsAny<string?>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
