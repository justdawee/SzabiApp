using SzabiApp.Backend.Models.Entities;

namespace SzabiApp.Backend.Services.Interfaces;

public interface IJwtFactory
{
    string GenerateToken(User user);
}
