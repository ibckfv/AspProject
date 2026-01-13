using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalServiceAspNet.Data;
using RentalServiceAspNet.Models;
using System.Security.Claims;

namespace RentalServiceAspNet.Controllers;

public class ApartmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApartmentsController> _logger;

    public ApartmentsController(ApplicationDbContext context, ILogger<ApartmentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Cities = cities;
        return View();
    }

    [Route("apartments/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        _logger.LogDebug("Запрос страницы детального просмотра объявления: ID {ApartmentId}", id);
        
        try
        {
            var apartment = await _context.Apartments
                .Include(a => a.City)
                .Include(a => a.Owner)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (apartment == null)
            {
                _logger.LogWarning("Объявление не найдено при запросе детальной страницы: ID {ApartmentId}", id);
                return NotFound();
            }

            _logger.LogDebug("Страница детального просмотра объявления загружена: ID {ApartmentId}, название: {Title}", 
                id, apartment.Title);
            return View(apartment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при загрузке детальной страницы объявления ID {ApartmentId}", id);
            return StatusCode(500);
        }
    }

    public async Task<IActionResult> Add()
    {
        var cities = await _context.Cities.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Cities = cities;
        return View();
    }

    [Authorize]
    public async Task<IActionResult> My()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
        {
            return RedirectToAction("Login", "Home");
        }

        var apartments = await _context.Apartments
            .Where(a => a.OwnerId == userId)
            .Include(a => a.City)
            .OrderByDescending(a => a.Id)
            .ToListAsync();

        return View(apartments);
    }
}
