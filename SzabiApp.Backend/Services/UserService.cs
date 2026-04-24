using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SzabiApp.Backend.Common;
using SzabiApp.Backend.Data.Context;
using SzabiApp.Backend.Mappings;
using SzabiApp.Backend.Models.DTOs.Users;
using SzabiApp.Backend.Models.Entities;
using SzabiApp.Backend.Services.Interfaces;

namespace SzabiApp.Backend.Services;

public class UserService(
    AppDbContext context,
    AppMapper mapper,
    IPasswordHasher<User> passwordHasher) : IUserService
{
    public async Task<Result<IEnumerable<UserDto>>> GetAllAsync()
    {
        var users = await context.Users
            .Include(u => u.Manager)
            .Include(u => u.WorkSchedule)
            .ToListAsync();

        return Result<IEnumerable<UserDto>>.Success(mapper.UsersToDto(users));
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id)
    {
        var user = await context.Users
            .Include(u => u.Manager)
            .Include(u => u.WorkSchedule)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return Result<UserDto>.NotFound($"User with ID '{id}' not found.");

        return Result<UserDto>.Success(mapper.UserToDto(user));
    }

    public async Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserDto request)
    {
        var user = await context.Users.FindAsync(id);
        if (user is null)
            return Result<UserDto>.NotFound($"User with ID '{id}' not found.");

        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;
        if (request.Email is not null) user.Email = request.Email;
        if (request.Role is not null) user.Role = request.Role.Value;
        if (request.IsActive is not null) user.IsActive = request.IsActive.Value;
        if (request.BirthDate is not null) user.BirthDate = request.BirthDate.Value;
        if (request.ManagerId is not null) user.ManagerId = request.ManagerId;
        if (request.WorkScheduleId is not null) user.WorkScheduleId = request.WorkScheduleId;

        await context.SaveChangesAsync();
        return Result<UserDto>.Success(mapper.UserToDto(user));
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordDto request)
    {
        var user = await context.Users.FindAsync(userId);
        if (user is null)
            return Result.NotFound("User not found.");

        var verify = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verify == PasswordVerificationResult.Failed)
            return Result.Failure("Current password is incorrect.");

        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        await context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var user = await context.Users.FindAsync(id);
        if (user is null)
            return Result.NotFound($"User with ID '{id}' not found.");

        user.IsActive = false;
        await context.SaveChangesAsync();

        return Result.Success();
    }
}
