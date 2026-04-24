using NodaTime;
using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Models.DTOs.Users;

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public LocalDate? BirthDate { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? WorkScheduleId { get; set; }
}
