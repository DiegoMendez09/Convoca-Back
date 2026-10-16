using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/manuals")]
public class ManualsController : ControllerBase
{
    private readonly IFunctionManualService _functionManualService;

    public ManualsController(IFunctionManualService functionManualService)
    {
        _functionManualService = functionManualService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var response = await _functionManualService.ListAsync(cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return Ok(response.Value);
    }

    [HttpPost]
    [AllowAnonymous]
    [RequestSizeLimit(33_554_432)]
    public async Task<IActionResult> Upload(IFormFile? file, [FromForm] string? opecCode, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Selecciona el PDF del manual." });

        await using var stream = file.OpenReadStream();
        var response = await _functionManualService.UploadAsync(stream, file.FileName, opecCode ?? string.Empty, cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return Ok(response.Value);
    }

    [HttpDelete("{manualId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(Guid manualId, CancellationToken cancellationToken)
    {
        var response = await _functionManualService.DeleteAsync(manualId, cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return NoContent();
    }
}
