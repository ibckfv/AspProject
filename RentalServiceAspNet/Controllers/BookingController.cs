using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalServiceAspNet.Models;
using RentalServiceAspNet.Controllers.RequestEntities;
using RentalServiceAspNet.Services;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace RentalServiceAspNet.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingController> _logger;

    public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Попытка создания бронирования без авторизации");
            return Unauthorized(new { ok = false, error = "Unauthorized" });
        }

        _logger.LogInformation("Попытка создания бронирования: пользователь {UserId}, объявление {ApartmentId}, даты {StartDate} - {EndDate}", 
            userId, request.ApartmentId, request.StartDate, request.EndDate);

        try
        {
            var result = await _bookingService.CreateAsync(request, userId);

            _logger.LogInformation("Бронирование успешно создано: ID {BookingId}, пользователь {UserId}, объявление {ApartmentId}, даты {StartDate} - {EndDate}, цена {TotalPrice}", 
                result.booking.Id, userId, request.ApartmentId, result.booking.StartDate, result.booking.EndDate, result.totalPrice);

            return Ok(new
            {
                ok = true,
                bookingId = result.booking.Id,
                totalPrice = result.totalPrice,
                days = result.days,
                message = "Бронирование успешно создано"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Бронирование отклонено: {Message}, пользователь {UserId}", ex.Message, userId);
            return BadRequest(new { ok = false, error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Бронирование отклонено: {Message}, пользователь {UserId}", ex.Message, userId);
            return NotFound(new { ok = false, error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Бронирование отклонено: {Message}, пользователь {UserId}", ex.Message, userId);
            return BadRequest(new { ok = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании бронирования пользователем {UserId}", userId);
            return StatusCode(500, new { ok = false, error = "Ошибка при создании бронирования" });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogDebug("Запрос списка всех бронирований");
        
        try
        {
            var bookings = await _bookingService.GetAllAsync();
            var result = bookings.Select(b => new
            {
                b.Id,
                b.ApartmentId,
                b.UserId,
                b.StartDate,
                b.EndDate,
                b.CreatedAt,
                ApartmentTitle = b.Apartment?.Title,
                UserName = b.User?.FullName
            }).ToList();

            _logger.LogInformation("Получен список всех бронирований: {Count} бронирований", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка всех бронирований");
            return StatusCode(500, new { error = "Ошибка при получении бронирований" });
        }
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Попытка получения списка бронирований без авторизации");
            return Unauthorized(new { error = "Unauthorized" });
        }

        _logger.LogDebug("Запрос списка бронирований пользователя: {UserId}", userId);

        try
        {
            var bookings = await _bookingService.GetByUserAsync(userId);
            var result = bookings.Select(b => new
            {
                b.Id,
                b.ApartmentId,
                b.StartDate,
                b.EndDate,
                b.CreatedAt,
                ApartmentTitle = b.Apartment?.Title,
                CityName = b.Apartment?.City?.Name
            }).ToList();

            _logger.LogInformation("Получен список бронирований пользователя {UserId}: {Count} бронирований", userId, result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка бронирований пользователя {UserId}", userId);
            return StatusCode(500, new { error = "Ошибка при получении бронирований" });
        }
    }

    [HttpGet("apartment/{apartmentId}")]
    public async Task<IActionResult> GetByApartment(int apartmentId)
    {
        _logger.LogDebug("Запрос бронирований для объявления: {ApartmentId}", apartmentId);
        
        try
        {
            var bookings = await _bookingService.GetByApartmentAsync(apartmentId);
            var result = bookings.Select(b => new
            {
                b.Id,
                b.StartDate,
                b.EndDate,
                UserName = b.User?.FullName
            }).ToList();

            _logger.LogDebug("Получено {Count} бронирований для объявления {ApartmentId}", result.Count, apartmentId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении бронирований для объявления {ApartmentId}", apartmentId);
            return StatusCode(500, new { error = "Ошибка при получении бронирований" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Попытка удаления бронирования без авторизации");
            return Unauthorized(new { ok = false, error = "Unauthorized" });
        }

        _logger.LogInformation("Попытка удаления бронирования: ID {BookingId}, пользователь {UserId}", id, userId);

        try
        {
            var deleted = await _bookingService.DeleteAsync(id, userId);
            if (!deleted)
            {
                _logger.LogWarning("Бронирование не найдено или нет прав: ID {BookingId}, пользователь {UserId}", id, userId);
                return NotFound(new { ok = false, error = "Бронирование не найдено" });
            }

            _logger.LogInformation("Бронирование успешно удалено: ID {BookingId}, пользователь {UserId}", id, userId);
            return Ok(new { ok = true, message = "Бронирование отменено" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении бронирования ID {BookingId}, пользователь {UserId}", id, userId);
            return StatusCode(500, new { ok = false, error = "Ошибка при отмене бронирования" });
        }
    }
}