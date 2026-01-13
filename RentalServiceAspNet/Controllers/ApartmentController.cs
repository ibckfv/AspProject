using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalServiceAspNet.Controllers.RequestEntities;
using RentalServiceAspNet.Models;
using RentalServiceAspNet.Services;
using System.Linq;

namespace RentalServiceAspNet.Controllers;

[ApiController]
[Route("api/apartments")]
public class ApartmentController : ControllerBase
{
    private readonly IApartmentService _apartmentService;
    private readonly ILogger<ApartmentController> _logger;

    public ApartmentController(IApartmentService apartmentService, ILogger<ApartmentController> logger)
    {
        _apartmentService = apartmentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            _logger.LogDebug("Запрос списка всех объявлений");
            var apartments = await _apartmentService.GetAllAsync();
            var result = apartments.Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                a.PricePerDay,
                a.MainPhoto,
                a.CityId,
                a.Rooms,
                a.Furnished,
                a.Parking,
                a.PetsAllowed,
                a.Internet,
                CityName = a.City?.Name,
                OwnerName = a.Owner?.FullName
            }).ToList();

            _logger.LogInformation("Получен список объявлений: {Count} объявлений", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка объявлений");
            return StatusCode(500, new { error = "Ошибка при получении объявлений" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogDebug("Запрос объявления по ID: {ApartmentId}", id);
        
        try
        {
            var apartment = await _apartmentService.GetByIdAsync(id);

            if (apartment == null)
            {
                _logger.LogWarning("Объявление не найдено: ID {ApartmentId}", id);
                return NotFound(new { error = "Not found" });
            }

            _logger.LogDebug("Объявление найдено: ID {ApartmentId}, название: {Title}", id, apartment.Title);
            return Ok(new
            {
                apartment.Id,
                apartment.Title,
                apartment.Description,
                apartment.PricePerDay,
                apartment.MainPhoto,
                apartment.CityId,
                apartment.OwnerId,
                apartment.Rooms,
                apartment.TotalArea,
                apartment.Floor,
                apartment.ViewType,
                apartment.Renovation,
                apartment.Lighting,
                apartment.Furnished,
                apartment.Parking,
                apartment.PetsAllowed,
                apartment.Internet,
                CityName = apartment.City?.Name,
                OwnerName = apartment.Owner?.FullName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении объявления ID {ApartmentId}", id);
            return StatusCode(500, new { error = "Ошибка при получении объявления" });
        }
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyApartments()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Попытка получения списка объявлений без авторизации");
            return Unauthorized(new { error = "Unauthorized" });
        }

        try
        {
            _logger.LogDebug("Запрос списка объявлений пользователя: {UserId}", userId);
            var apartments = await _apartmentService.GetByOwnerAsync(userId);
            var result = apartments.Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                a.PricePerDay,
                a.MainPhoto,
                a.CreatedAt,
                CityName = a.City?.Name
            }).ToList();

            _logger.LogInformation("Получен список объявлений пользователя {UserId}: {Count} объявлений", userId, result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка объявлений пользователя {UserId}", userId);
            return StatusCode(500, new { error = "Ошибка при получении объявлений" });
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateApartmentRequest request)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Попытка создания объявления без авторизации");
            return Unauthorized(new { ok = false, error = "Unauthorized" });
        }

        _logger.LogInformation("Попытка создания объявления пользователем {UserId}, название: {Title}", userId, request.Title);

        if (string.IsNullOrWhiteSpace(request.Title) || request.PricePerDay <= 0)
        {
            _logger.LogWarning("Создание объявления отклонено: не заполнены обязательные поля (пользователь {UserId})", userId);
            return BadRequest(new { ok = false, error = "Название и цена обязательны" });
        }

        try
        {
            var apartment = await _apartmentService.CreateAsync(request, userId);
            _logger.LogInformation("Объявление успешно создано: ID {ApartmentId}, пользователь {UserId}, название: {Title}", 
                apartment.Id, userId, request.Title);
            return Ok(new { ok = true, apartmentId = apartment.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании объявления пользователем {UserId}", userId);
            return StatusCode(500, new { ok = false, error = "Ошибка при создании объявления" });
        }
    }
}
