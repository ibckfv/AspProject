using Microsoft.AspNetCore.Mvc;
using RentalServiceAspNet.Data;
using RentalServiceAspNet.Models;

namespace RentalServiceAspNet.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var apartments = _context.Apartments
            .Where(a => a.Status == null || a.Status == "approved")
            .Take(5)
            .ToList();
        return View(apartments);
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    public IActionResult Profile()
    {
        return View();
    }
}
