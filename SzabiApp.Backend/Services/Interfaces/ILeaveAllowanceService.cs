using SzabiApp.Backend.Common;
using SzabiApp.Backend.Models.DTOs.LeaveAllowances;

namespace SzabiApp.Backend.Services.Interfaces;

public interface ILeaveAllowanceService
{
    Task<Result<IEnumerable<LeaveAllowanceDto>>> GetAllAsync();
    Task<Result<IEnumerable<LeaveAllowanceDto>>> GetByUserIdAsync(Guid userId);
    Task<Result<LeaveAllowanceDto>> CreateAsync(CreateLeaveAllowanceDto request);
    Task<Result<LeaveAllowanceDto>> UpdateAsync(Guid id, UpdateLeaveAllowanceDto request);
    Task<Result> DeleteAsync(Guid id);
}
