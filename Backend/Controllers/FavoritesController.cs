using Backend.Models.DTOs.Requests;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/v1/favorites")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var response = await _favoriteService.GetAllAsync(cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return Ok(response.Value);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateFavoriteRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _favoriteService.CreateAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return StatusCode(response.StatusCode, response.Value);
    }

    [HttpDelete("{processId}")]
    [AllowAnonymous]
    public async Task<IActionResult> Delete(string processId, CancellationToken cancellationToken)
    {
        var response = await _favoriteService.DeleteAsync(processId, cancellationToken);
        if (!response.IsSuccess)
            return StatusCode(response.StatusCode, new { message = response.Error });

        return Ok(new { removed = true });
    }
}
