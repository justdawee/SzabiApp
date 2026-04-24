using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using SzabiApp.Backend.Common;
using SzabiApp.Backend.Data.Context;
using SzabiApp.Backend.Mappings;
using SzabiApp.Backend.Models.DTOs.Auth;
using SzabiApp.Backend.Models.DTOs.Users;
using SzabiApp.Backend.Models.Entities;
using SzabiApp.Backend.Services.Interfaces;

namespace SzabiApp.Backend.Services;

public class AuthService(
    AppDbContext context,
    IJwtFactory jwtFactory,
    IPasswordHasher<User> passwordHasher,
    AppMapper mapper,
    IClock clock) : IAuthService
{
    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (user is null)
            return Result<AuthResponseDto>.Unauthorized("Invalid email or password.");

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Result<AuthResponseDto>.Unauthorized("Invalid email or password.");

        var token = jwtFactory.GenerateToken(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            FullName = $"{user.LastName} {user.FirstName}",
            Role = user.Role.ToString()
        });
    }

    public async Task<Result<UserDto>> RegisterAsync(RegisterRequestDto request)
    {
        var emailExists = await context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
            return Result<UserDto>.Conflict($"Email '{request.Email}' is already in use.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Role = request.Role,
            BirthDate = request.BirthDate,
            ManagerId = request.ManagerId,
            WorkScheduleId = request.WorkScheduleId,
            IsActive = true,
            CreatedAt = clock.GetCurrentInstant().InUtc().LocalDateTime
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return Result<UserDto>.Created(mapper.UserToDto(user));
    }
}
