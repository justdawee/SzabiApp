using Microsoft.EntityFrameworkCore;
using SzabiApp.Backend.Common;
using SzabiApp.Backend.Data.Context;
using SzabiApp.Backend.Mappings;
using SzabiApp.Backend.Models.DTOs.LeaveAllowances;
using SzabiApp.Backend.Models.Entities;
using SzabiApp.Backend.Services.Interfaces;

namespace SzabiApp.Backend.Services;

public class LeaveAllowanceService(AppDbContext context, AppMapper mapper) : ILeaveAllowanceService
{
    public async Task<Result<IEnumerable<LeaveAllowanceDto>>> GetAllAsync()
    {
        var allowances = await context.LeaveAllowances
            .Include(a => a.User)
            .ToListAsync();

        return Result<IEnumerable<LeaveAllowanceDto>>.Success(mapper.LeaveAllowancesToDto(allowances));
    }

    public async Task<Result<IEnumerable<LeaveAllowanceDto>>> GetByUserIdAsync(Guid userId)
    {
        var allowances = await context.LeaveAllowances
            .Include(a => a.User)
            .Where(a => a.UserId == userId)
            .ToListAsync();

        return Result<IEnumerable<LeaveAllowanceDto>>.Success(mapper.LeaveAllowancesToDto(allowances));
    }

    public async Task<Result<LeaveAllowanceDto>> CreateAsync(CreateLeaveAllowanceDto request)
    {
        var userExists = await context.Users.AnyAsync(u => u.Id == request.UserId && u.IsActive);
        if (!userExists)
            return Result<LeaveAllowanceDto>.NotFound($"Active user with ID '{request.UserId}' not found.");

        var duplicate = await context.LeaveAllowances.AnyAsync(a =>
            a.UserId == request.UserId &&
            a.Year == request.Year &&
            a.Category == request.Category);

        if (duplicate)
            return Result<LeaveAllowanceDto>.Conflict(
                $"An allowance for '{request.Category}' in {request.Year} already exists for this user.");

        var allowance = new LeaveAllowance
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Year = request.Year,
            Category = request.Category,
            TotalDays = request.TotalDays,
            UsedDays = 0
        };

        context.LeaveAllowances.Add(allowance);
        await context.SaveChangesAsync();

        await context.Entry(allowance).Reference(a => a.User).LoadAsync();

        return Result<LeaveAllowanceDto>.Created(mapper.LeaveAllowanceToDto(allowance));
    }

    public async Task<Result<LeaveAllowanceDto>> UpdateAsync(Guid id, UpdateLeaveAllowanceDto request)
    {
        var allowance = await context.LeaveAllowances
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (allowance is null)
            return Result<LeaveAllowanceDto>.NotFound($"Leave allowance with ID '{id}' not found.");

        if (request.TotalDays < allowance.UsedDays)
            return Result<LeaveAllowanceDto>.Failure(
                $"Total days ({request.TotalDays}) cannot be less than already used days ({allowance.UsedDays}).");

        allowance.TotalDays = request.TotalDays;
        await context.SaveChangesAsync();

        return Result<LeaveAllowanceDto>.Success(mapper.LeaveAllowanceToDto(allowance));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var allowance = await context.LeaveAllowances.FindAsync(id);
        if (allowance is null)
            return Result.NotFound($"Leave allowance with ID '{id}' not found.");

        context.LeaveAllowances.Remove(allowance);
        await context.SaveChangesAsync();

        return Result.Success();
    }
}
