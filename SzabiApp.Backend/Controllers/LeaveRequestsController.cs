using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SzabiApp.Backend.Models.DTOs.LeaveRequests;
using SzabiApp.Backend.Services.Interfaces;

namespace SzabiApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("standard")]
public class LeaveRequestsController(ILeaveRequestService leaveRequestService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> GetAll()
    {
        if (User.IsInRole("Admin"))
        {
            var result = await leaveRequestService.GetAllAsync();
            return Ok(result.Value);
        }

        // Manager: only subordinates' requests
        var managerId = GetCurrentUserId();
        if (managerId is null) return Unauthorized();

        var managerResult = await leaveRequestService.GetByManagerAsync(managerId.Value);
        return Ok(managerResult.Value);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMine()
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await leaveRequestService.GetByUserIdAsync(userId.Value);
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await leaveRequestService.GetByIdAsync(id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        // Employees can only view their own requests
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            var userId = GetCurrentUserId();
            if (result.Value!.UserId != userId)
                return Forbid();
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await leaveRequestService.CreateAsync(userId.Value, request);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return StatusCode(201, result.Value);
    }

    [HttpPatch("{id:guid}/review")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewLeaveRequestDto request)
    {
        var reviewerId = GetCurrentUserId();
        if (reviewerId is null) return Unauthorized();

        var result = await leaveRequestService.ReviewAsync(id, reviewerId.Value, request);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await leaveRequestService.CancelAsync(id, userId.Value);
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
