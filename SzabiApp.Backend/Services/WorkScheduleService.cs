using Microsoft.EntityFrameworkCore;
using SzabiApp.Backend.Common;
using SzabiApp.Backend.Data.Context;
using SzabiApp.Backend.Mappings;
using SzabiApp.Backend.Models.DTOs.WorkSchedules;
using SzabiApp.Backend.Models.Entities;
using SzabiApp.Backend.Services.Interfaces;

namespace SzabiApp.Backend.Services;

public class WorkScheduleService(AppDbContext context, AppMapper mapper) : IWorkScheduleService
{
    public async Task<Result<IEnumerable<WorkScheduleDto>>> GetAllAsync()
    {
        var schedules = await context.WorkSchedules.ToListAsync();
        return Result<IEnumerable<WorkScheduleDto>>.Success(mapper.WorkSchedulesToDto(schedules));
    }

    public async Task<Result<WorkScheduleDto>> GetByIdAsync(Guid id)
    {
        var schedule = await context.WorkSchedules.FindAsync(id);
        if (schedule is null)
            return Result<WorkScheduleDto>.NotFound($"Work schedule with ID '{id}' not found.");

        return Result<WorkScheduleDto>.Success(mapper.WorkScheduleToDto(schedule));
    }

    public async Task<Result<WorkScheduleDto>> CreateAsync(CreateWorkScheduleDto request)
    {
        var nameExists = await context.WorkSchedules.AnyAsync(ws => ws.Name == request.Name);
        if (nameExists)
            return Result<WorkScheduleDto>.Conflict($"A work schedule named '{request.Name}' already exists.");

        var schedule = new WorkSchedule
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            WorkDaysPerWeek = request.WorkDaysPerWeek,
            DailyWorkHours = request.DailyWorkHours
        };

        context.WorkSchedules.Add(schedule);
        await context.SaveChangesAsync();

        return Result<WorkScheduleDto>.Created(mapper.WorkScheduleToDto(schedule));
    }

    public async Task<Result<WorkScheduleDto>> UpdateAsync(Guid id, UpdateWorkScheduleDto request)
    {
        var schedule = await context.WorkSchedules.FindAsync(id);
        if (schedule is null)
            return Result<WorkScheduleDto>.NotFound($"Work schedule with ID '{id}' not found.");

        if (request.Name is not null) schedule.Name = request.Name;
        if (request.WorkDaysPerWeek is not null) schedule.WorkDaysPerWeek = request.WorkDaysPerWeek.Value;
        if (request.DailyWorkHours is not null) schedule.DailyWorkHours = request.DailyWorkHours.Value;

        await context.SaveChangesAsync();
        return Result<WorkScheduleDto>.Success(mapper.WorkScheduleToDto(schedule));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var schedule = await context.WorkSchedules
            .Include(ws => ws.Users)
            .FirstOrDefaultAsync(ws => ws.Id == id);

        if (schedule is null)
            return Result.NotFound($"Work schedule with ID '{id}' not found.");

        if (schedule.Users.Count > 0)
            return Result.Failure($"Cannot delete: {schedule.Users.Count} user(s) are assigned to this schedule.");

        context.WorkSchedules.Remove(schedule);
        await context.SaveChangesAsync();

        return Result.Success();
    }
}
