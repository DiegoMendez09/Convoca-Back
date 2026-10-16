using Backend.Models.DTOs.Requests;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/exams")]
public class ExamsController : ControllerBase
{
    private readonly IMeritExamService _meritExamService;

    public ExamsController(IMeritExamService meritExamService)
    {
        _meritExamService = meritExamService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var response = await _meritExamService.ListAsync(cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return Ok(response.Value);
    }

    [HttpGet("{examId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid examId, CancellationToken cancellationToken)
    {
        var response = await _meritExamService.GetAsync(examId, cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return Ok(response.Value);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Generate([FromBody] GenerateExamRequest request, CancellationToken cancellationToken)
    {
        var response = await _meritExamService.GenerateAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return Ok(response.Value);
    }

    [HttpDelete("{examId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(Guid examId, CancellationToken cancellationToken)
    {
        var response = await _meritExamService.DeleteAsync(examId, cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return NoContent();
    }
}
