using SzabiApp.Backend.Common;
using SzabiApp.Backend.Models.DTOs.Auth;
using SzabiApp.Backend.Models.DTOs.Users;

namespace SzabiApp.Backend.Services.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<Result<UserDto>> RegisterAsync(RegisterRequestDto request);
}
