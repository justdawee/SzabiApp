using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SzabiApp.Backend.Data.Context;
using SzabiApp.Backend.Models.DTOs.Holidays;
using SzabiApp.Backend.Models.Entities;

namespace SzabiApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("standard")]
public class HolidaysController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var holidays = await context.Holidays.ToListAsync();
        return Ok(holidays);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateHolidayDto request)
    {
        var holiday = new Holiday
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Date = request.Date,
            IsRecurringYearly = request.IsRecurringYearly
        };

        context.Holidays.Add(holiday);
        await context.SaveChangesAsync();

        return StatusCode(201, holiday);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var holiday = await context.Holidays.FindAsync(id);
        if (holiday is null)
            return NotFound(new { error = $"Holiday with ID '{id}' not found." });

        context.Holidays.Remove(holiday);
        await context.SaveChangesAsync();

        return NoContent();
    }
}
