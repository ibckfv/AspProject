using RentalServiceAspNet.Controllers.RequestEntities;
using RentalServiceAspNet.Models;

namespace RentalServiceAspNet.Services;

public interface IAuthService
{
    Task<(bool ok, string? error)> RegisterAsync(RegisterRequest request);
    Task<(bool ok, string? error, string? token)> LoginAsync(LoginRequest request);
    Task<User?> GetUserAsync(long userId);
    Task<(bool ok, string? error)> UpdateProfileAsync(long userId, UpdateProfileRequest request);
}