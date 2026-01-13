using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalServiceAspNet.Controllers.RequestEntities;
using RentalServiceAspNet.Data;
using RentalServiceAspNet.Models;

namespace RentalServiceAspNet.Services;

public class ApartmentService : IApartmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApartmentService> _logger;

    public ApartmentService(ApplicationDbContext context, ILogger<ApartmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Apartment>> GetAllAsync()
    {
        _logger.LogDebug("Получение списка всех объявлений");

        return await _context.Apartments
            .Include(a => a.City)
            .Include(a => a.Owner)
            .OrderByDescending(a => a.Id)
            .ToListAsync();
    }

    public async Task<Apartment?> GetByIdAsync(int id)
    {
        _logger.LogDebug("Получение объявления по ID {ApartmentId}", id);

        return await _context.Apartments
            .Include(a => a.City)
            .Include(a => a.Owner)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Apartment>> GetByOwnerAsync(long ownerId)
    {
        _logger.LogDebug("Получение объявлений владельца {OwnerId}", ownerId);

        return await _context.Apartments
            .Where(a => a.OwnerId == ownerId)
            .Include(a => a.City)
            .OrderByDescending(a => a.Id)
            .ToListAsync();
    }

    public async Task<Apartment> CreateAsync(CreateApartmentRequest request, long ownerId)
    {
        _logger.LogInformation("Создание объявления пользователем {OwnerId}, название {Title}", ownerId, request.Title);

        var apartment = new Apartment
        {
            Title = request.Title,
            Description = request.Description,
            PricePerDay = request.PricePerDay,
            CityId = request.CityId > 0 ? request.CityId : null,
            OwnerId = ownerId,
            ViewType = request.ViewType,
            Lighting = request.Lighting,
            Renovation = request.Renovation,
            Floor = request.Floor,
            TotalArea = request.TotalArea,
            Rooms = request.Rooms,
            Furnished = request.Furnished,
            PetsAllowed = request.PetsAllowed,
            Parking = request.Parking,
            Internet = request.Internet,
            CreatedAt = DateTime.UtcNow
        };

        _context.Apartments.Add(apartment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Объявление создано: ID {ApartmentId}", apartment.Id);
        return apartment;
    }
}
