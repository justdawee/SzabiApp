using Microsoft.EntityFrameworkCore;
using NodaTime;
using SzabiApp.Backend.Common;
using SzabiApp.Backend.Data.Context;
using SzabiApp.Backend.Mappings;
using SzabiApp.Backend.Models.DTOs.LeaveRequests;
using SzabiApp.Backend.Models.Entities;
using SzabiApp.Backend.Models.Enums;
using SzabiApp.Backend.Services.Interfaces;

namespace SzabiApp.Backend.Services;

public class LeaveRequestService(AppDbContext context, AppMapper mapper, IClock clock) : ILeaveRequestService
{
    public async Task<Result<IEnumerable<LeaveRequestDto>>> GetAllAsync()
    {
        var requests = await context.LeaveRequests
            .Include(lr => lr.User)
            .Include(lr => lr.ReviewedBy)
            .ToListAsync();

        return Result<IEnumerable<LeaveRequestDto>>.Success(mapper.LeaveRequestsToDto(requests));
    }

    public async Task<Result<IEnumerable<LeaveRequestDto>>> GetByManagerAsync(Guid managerId)
    {
        var subordinateIds = await context.Users
            .Where(u => u.ManagerId == managerId && u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();

        var requests = await context.LeaveRequests
            .Include(lr => lr.User)
            .Include(lr => lr.ReviewedBy)
            .Where(lr => subordinateIds.Contains(lr.UserId))
            .ToListAsync();

        return Result<IEnumerable<LeaveRequestDto>>.Success(mapper.LeaveRequestsToDto(requests));
    }

    public async Task<Result<IEnumerable<LeaveRequestDto>>> GetByUserIdAsync(Guid userId)
    {
        var requests = await context.LeaveRequests
            .Include(lr => lr.User)
            .Include(lr => lr.ReviewedBy)
            .Where(lr => lr.UserId == userId)
            .ToListAsync();

        return Result<IEnumerable<LeaveRequestDto>>.Success(mapper.LeaveRequestsToDto(requests));
    }

    public async Task<Result<LeaveRequestDto>> GetByIdAsync(Guid id)
    {
        var request = await context.LeaveRequests
            .Include(lr => lr.User)
            .Include(lr => lr.ReviewedBy)
            .FirstOrDefaultAsync(lr => lr.Id == id);

        if (request is null)
            return Result<LeaveRequestDto>.NotFound($"Leave request with ID '{id}' not found.");

        return Result<LeaveRequestDto>.Success(mapper.LeaveRequestToDto(request));
    }

    public async Task<Result<LeaveRequestDto>> CreateAsync(Guid userId, CreateLeaveRequestDto request)
    {
        var workingDays = await CalculateWorkingDaysAsync(request.StartDate, request.EndDate);

        if (workingDays == 0)
            return Result<LeaveRequestDto>.Failure("The selected period contains no working days.");

        var allowance = await context.LeaveAllowances
            .FirstOrDefaultAsync(a =>
                a.UserId == userId &&
                a.Year == request.StartDate.Year &&
                a.Category == request.Category);

        if (allowance is null)
            return Result<LeaveRequestDto>.Failure(
                $"No leave allowance found for category '{request.Category}' in {request.StartDate.Year}. Contact your administrator.");

        if (allowance.RemainingDays < workingDays)
            return Result<LeaveRequestDto>.Failure(
                $"Insufficient leave balance. Requested: {workingDays} working day(s), available: {allowance.RemainingDays}.");

        var leaveRequest = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Category = request.Category,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = LeaveStatus.Pending,
            RequestNote = request.RequestNote,
            CreatedAt = clock.GetCurrentInstant().InUtc().LocalDateTime
        };

        context.LeaveRequests.Add(leaveRequest);
        await context.SaveChangesAsync();

        await context.Entry(leaveRequest).Reference(lr => lr.User).LoadAsync();

        return Result<LeaveRequestDto>.Created(mapper.LeaveRequestToDto(leaveRequest));
    }

    public async Task<Result<LeaveRequestDto>> ReviewAsync(Guid id, Guid reviewerId, ReviewLeaveRequestDto request)
    {
        var leaveRequest = await context.LeaveRequests
            .Include(lr => lr.User)
            .FirstOrDefaultAsync(lr => lr.Id == id);

        if (leaveRequest is null)
            return Result<LeaveRequestDto>.NotFound($"Leave request with ID '{id}' not found.");

        if (leaveRequest.Status != LeaveStatus.Pending)
            return Result<LeaveRequestDto>.Failure("Only pending requests can be reviewed.");

        if (request.Decision == LeaveStatus.Approved)
        {
            var workingDays = await CalculateWorkingDaysAsync(leaveRequest.StartDate, leaveRequest.EndDate);

            var allowance = await context.LeaveAllowances
                .FirstOrDefaultAsync(a =>
                    a.UserId == leaveRequest.UserId &&
                    a.Year == leaveRequest.StartDate.Year &&
                    a.Category == leaveRequest.Category);

            if (allowance is null)
                return Result<LeaveRequestDto>.Failure(
                    $"No leave allowance found for this user. Cannot approve.");

            if (allowance.RemainingDays < workingDays)
                return Result<LeaveRequestDto>.Failure(
                    $"Cannot approve: insufficient leave balance. Required: {workingDays} day(s), available: {allowance.RemainingDays}.");

            allowance.UsedDays += workingDays;
        }

        leaveRequest.Status = request.Decision;
        leaveRequest.ReviewNote = request.ReviewNote;
        leaveRequest.ReviewedById = reviewerId;
        leaveRequest.ReviewedAt = clock.GetCurrentInstant().InUtc().LocalDateTime;

        await context.SaveChangesAsync();

        await context.Entry(leaveRequest).Reference(lr => lr.ReviewedBy).LoadAsync();

        return Result<LeaveRequestDto>.Success(mapper.LeaveRequestToDto(leaveRequest));
    }

    public async Task<Result> CancelAsync(Guid id, Guid userId)
    {
        var leaveRequest = await context.LeaveRequests.FindAsync(id);

        if (leaveRequest is null)
            return Result.NotFound($"Leave request with ID '{id}' not found.");

        if (leaveRequest.UserId != userId)
            return Result.Forbidden("You can only cancel your own leave requests.");

        if (leaveRequest.Status != LeaveStatus.Pending && leaveRequest.Status != LeaveStatus.Approved)
            return Result.Failure("Only pending or approved requests can be cancelled.");

        if (leaveRequest.Status == LeaveStatus.Approved)
        {
            var workingDays = await CalculateWorkingDaysAsync(leaveRequest.StartDate, leaveRequest.EndDate);

            var allowance = await context.LeaveAllowances
                .FirstOrDefaultAsync(a =>
                    a.UserId == leaveRequest.UserId &&
                    a.Year == leaveRequest.StartDate.Year &&
                    a.Category == leaveRequest.Category);

            if (allowance is not null)
                allowance.UsedDays = Math.Max(0, allowance.UsedDays - workingDays);
        }

        leaveRequest.Status = LeaveStatus.Cancelled;
        await context.SaveChangesAsync();

        return Result.Success();
    }

    // Counts working days between start and end (inclusive),
    // excluding weekends and public holidays (recurring holidays match on month+day).
    private async Task<int> CalculateWorkingDaysAsync(LocalDate start, LocalDate end)
    {
        var holidays = await context.Holidays.ToListAsync();

        var workingDays = 0;
        var current = start;

        while (current <= end)
        {
            var isWeekend = current.DayOfWeek is IsoDayOfWeek.Saturday or IsoDayOfWeek.Sunday;

            var isHoliday = holidays.Any(h =>
                h.IsRecurringYearly
                    ? h.Date.Month == current.Month && h.Date.Day == current.Day
                    : h.Date == current);

            if (!isWeekend && !isHoliday)
                workingDays++;

            current = current.PlusDays(1);
        }

        return workingDays;
    }
}
