using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalServiceAspNet.Controllers.RequestEntities;
using RentalServiceAspNet.Data;
using RentalServiceAspNet.Models;

namespace RentalServiceAspNet.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<(bool ok, string? error)> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FullName))
        {
            return (false, "Все поля обязательны");
        }

        if (await _context.Users.AnyAsync(u => u.Login == request.Login))
        {
            return (false, "Пользователь уже существует");
        }

        var user = new User
        {
            Login = request.Login,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName,
            Role = "user",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Пользователь зарегистрирован: {Login}", request.Login);
        return (true, null);
    }

    public async Task<(bool ok, string? error, string? token)> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, "Логин и пароль обязательны", null);
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == request.Login);
        if (user == null)
        {
            return (false, "Пользователь не найден", null);
        }

        if (!user.IsActive)
        {
            return (false, "Аккаунт заблокирован", null);
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash ?? ""))
        {
            return (false, "Неверный пароль", null);
        }

        var token = _jwtService.GenerateToken(user.Id, user.Login ?? "", user.Role ?? "user");
        return (true, null, token);
    }

    public async Task<User?> GetUserAsync(long userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<(bool ok, string? error)> UpdateProfileAsync(long userId, UpdateProfileRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return (false, "User not found");
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.FullName = request.FullName;
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }
}
