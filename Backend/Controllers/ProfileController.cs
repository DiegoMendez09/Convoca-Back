using Backend.Models.DTOs.Requests;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/profile")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var response = await _profileService.GetAsync(cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return Ok(response.Value);
    }

    [HttpPut]
    [AllowAnonymous]
    public async Task<IActionResult> Save([FromBody] SaveProfileRequest request, CancellationToken cancellationToken)
    {
        var response = await _profileService.SaveAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return Ok(response.Value);
    }
}
