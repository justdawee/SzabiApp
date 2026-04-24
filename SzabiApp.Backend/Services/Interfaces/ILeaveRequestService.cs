using SzabiApp.Backend.Common;
using SzabiApp.Backend.Models.DTOs.LeaveRequests;

namespace SzabiApp.Backend.Services.Interfaces;

public interface ILeaveRequestService
{
    Task<Result<IEnumerable<LeaveRequestDto>>> GetAllAsync();
    Task<Result<IEnumerable<LeaveRequestDto>>> GetByManagerAsync(Guid managerId);
    Task<Result<IEnumerable<LeaveRequestDto>>> GetByUserIdAsync(Guid userId);
    Task<Result<LeaveRequestDto>> GetByIdAsync(Guid id);
    Task<Result<LeaveRequestDto>> CreateAsync(Guid userId, CreateLeaveRequestDto request);
    Task<Result<LeaveRequestDto>> ReviewAsync(Guid id, Guid reviewerId, ReviewLeaveRequestDto request);
    Task<Result> CancelAsync(Guid id, Guid userId);
}
