using SzabiApp.Backend.Common;
using SzabiApp.Backend.Models.DTOs.Users;

namespace SzabiApp.Backend.Services.Interfaces;

public interface IUserService
{
    Task<Result<IEnumerable<UserDto>>> GetAllAsync();
    Task<Result<UserDto>> GetByIdAsync(Guid id);
    Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserDto request);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto request);
    Task<Result> DeleteAsync(Guid id);
}
