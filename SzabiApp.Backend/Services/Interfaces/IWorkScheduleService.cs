using SzabiApp.Backend.Common;
using SzabiApp.Backend.Models.DTOs.WorkSchedules;

namespace SzabiApp.Backend.Services.Interfaces;

public interface IWorkScheduleService
{
    Task<Result<IEnumerable<WorkScheduleDto>>> GetAllAsync();
    Task<Result<WorkScheduleDto>> GetByIdAsync(Guid id);
    Task<Result<WorkScheduleDto>> CreateAsync(CreateWorkScheduleDto request);
    Task<Result<WorkScheduleDto>> UpdateAsync(Guid id, UpdateWorkScheduleDto request);
    Task<Result> DeleteAsync(Guid id);
}
