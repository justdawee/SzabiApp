using NodaTime;
using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Models.DTOs.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public LocalDate BirthDate { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerFullName { get; set; }
    public Guid? WorkScheduleId { get; set; }
    public string? WorkScheduleName { get; set; }
}
