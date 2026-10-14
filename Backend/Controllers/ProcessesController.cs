using Backend.Models.DTOs.Requests;
using Backend.Options;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/processes")]
public class ProcessesController : ControllerBase
{
    private readonly IProcessService _processService;

    public ProcessesController(IProcessService processService)
    {
        _processService = processService;
    }

    /// <summary>
    /// La consulta es anónima porque el conjunto p6dx-8zbt es dato abierto de SECOP II.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] ProcessSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await _processService.SearchAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return Ok(response.Value);
    }

    [HttpGet("export")]
    [AllowAnonymous]
    public async Task<IActionResult> Export([FromQuery] ProcessSearchRequest request, CancellationToken cancellationToken)
    {
        var response = await _processService.ExportAsync(request, cancellationToken);
        if (!response.IsSuccess || response.Value is null)
            return StatusCode(response.StatusCode, new { message = response.Error });

        Response.Headers[CorsSettings.ExportTruncatedHeader] = response.Value.Truncated ? "true" : "false";
        Response.Headers[CorsSettings.ExportCountHeader] = response.Value.ExportedCount.ToString();
        return File(response.Value.Content, "text/csv; charset=utf-8", response.Value.FileName);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var response = await _processService.GetByIdAsync(id, cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return Ok(response.Value);
    }
}
