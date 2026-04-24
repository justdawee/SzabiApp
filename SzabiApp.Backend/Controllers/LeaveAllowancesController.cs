using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SzabiApp.Backend.Models.DTOs.LeaveAllowances;
using SzabiApp.Backend.Services.Interfaces;

namespace SzabiApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("standard")]
public class LeaveAllowancesController(ILeaveAllowanceService leaveAllowanceService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var result = await leaveAllowanceService.GetAllAsync();
        return Ok(result.Value);
    }

    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        var result = await leaveAllowanceService.GetByUserIdAsync(userId);
        return Ok(result.Value);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMine()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await leaveAllowanceService.GetByUserIdAsync(userId.Value);
        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateLeaveAllowanceDto request)
    {
        var result = await leaveAllowanceService.CreateAsync(request);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return StatusCode(201, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeaveAllowanceDto request)
    {
        var result = await leaveAllowanceService.UpdateAsync(id, request);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await leaveAllowanceService.DeleteAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return NoContent();
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
