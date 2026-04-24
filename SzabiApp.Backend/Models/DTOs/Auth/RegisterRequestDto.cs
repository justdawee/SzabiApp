using NodaTime;
using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Models.DTOs.Auth;

public class RegisterRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public LocalDate BirthDate { get; set; }
    public UserRole Role { get; set; } = UserRole.Employee;
    public Guid? ManagerId { get; set; }
    public Guid? WorkScheduleId { get; set; }
}
