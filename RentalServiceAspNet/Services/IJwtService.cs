using System.Security.Claims;

namespace RentalServiceAspNet.Services;

public interface IJwtService
{
    string GenerateToken(long userId, string login, string role);
    ClaimsPrincipal? ValidateToken(string token);
}
