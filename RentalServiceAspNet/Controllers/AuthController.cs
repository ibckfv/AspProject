using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalServiceAspNet.Models;
using RentalServiceAspNet.Controllers.RequestEntities;
using RentalServiceAspNet.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace RentalServiceAspNet.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Попытка регистрации пользователя: {Login}", request.Login);

        try
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.ok)
            {
                _logger.LogWarning("Регистрация отклонена: {Error}", result.error);
                return BadRequest(new { ok = false, error = result.error });
            }

            _logger.LogInformation("Пользователь успешно зарегистрирован: {Login}", request.Login);
            return Ok(new { ok = true, message = "Пользователь зарегистрирован" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации пользователя {Login}", request.Login);
            return StatusCode(500, new { ok = false, error = "Ошибка при регистрации" });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Попытка входа пользователя: {Login}", request.Login);

        try
        {
            var result = await _authService.LoginAsync(request);
            if (!result.ok || string.IsNullOrEmpty(result.token))
            {
                _logger.LogWarning("Вход отклонен: {Error}", result.error);
                return BadRequest(new { ok = false, error = result.error });
            }

            _logger.LogInformation("Пользователь {Login} успешно вошел в систему", request.Login);
            return Ok(new { ok = true, token = result.token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при генерации токена для пользователя {Login}", request.Login);
            return StatusCode(500, new { ok = false, error = "Ошибка при входе" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Попытка получения профиля без авторизации");
            return Unauthorized(new { ok = false, error = "Unauthorized" });
        }

        _logger.LogDebug("Запрос профиля пользователя: {UserId}", userId);

        try
        {
            var user = await _authService.GetUserAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Пользователь не найден: ID {UserId}", userId);
                return NotFound(new { ok = false, error = "User not found" });
            }

            _logger.LogDebug("Профиль пользователя получен: {UserId}, логин: {Login}", userId, user.Login);
            return Ok(new
            {
                ok = true,
                user = new
                {
                    Id = user.Id,
                    Login = user.Login,
                    FullName = user.FullName,
                    Role = user.Role
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении профиля пользователя {UserId}", userId);
            return StatusCode(500, new { ok = false, error = "Ошибка при получении профиля" });
        }
    }

    [HttpPut("update")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Попытка обновления профиля без авторизации");
            return Unauthorized(new { ok = false, error = "Unauthorized" });
        }

        _logger.LogInformation("Попытка обновления профиля пользователя: {UserId}", userId);

        try
        {
            var result = await _authService.UpdateProfileAsync(userId, request);
            if (!result.ok)
            {
                _logger.LogWarning("Ошибка при обновлении профиля: {Error}", result.error);
                return NotFound(new { ok = false, error = result.error });
            }

            _logger.LogInformation("Профиль пользователя {UserId} успешно обновлен", userId);
            return Ok(new { ok = true, message = "Профиль обновлен" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении профиля пользователя {UserId}", userId);
            return StatusCode(500, new { ok = false, error = "Ошибка при обновлении профиля" });
        }
    }
}