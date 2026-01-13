using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalServiceAspNet.Controllers.RequestEntities;
using RentalServiceAspNet.Data;
using RentalServiceAspNet.Models;

namespace RentalServiceAspNet.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BookingService> _logger;

    public BookingService(ApplicationDbContext context, ILogger<BookingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(Booking booking, int days, int totalPrice)> CreateAsync(CreateBookingRequest request, long userId)
    {
        if (!DateTime.TryParse(request.StartDate, out var startDate) ||
            !DateTime.TryParse(request.EndDate, out var endDate))
        {
            throw new ArgumentException("Неверный формат даты");
        }

        if (startDate < DateTime.Today)
        {
            throw new ArgumentException("Дата начала не может быть в прошлом");
        }

        if (endDate <= startDate)
        {
            throw new ArgumentException("Дата окончания должна быть позже даты начала");
        }

        var apartment = await _context.Apartments.FindAsync(request.ApartmentId);
        if (apartment == null)
        {
            throw new KeyNotFoundException("Квартира не найдена");
        }

        var conflictingBookings = await _context.Bookings
            .Where(b => b.ApartmentId == request.ApartmentId &&
                       ((b.StartDate <= startDate && b.EndDate >= startDate) ||
                        (b.StartDate <= endDate && b.EndDate >= endDate) ||
                        (b.StartDate >= startDate && b.EndDate <= endDate)))
            .AnyAsync();

        if (conflictingBookings)
        {
            throw new InvalidOperationException("Квартира уже забронирована на выбранные даты");
        }

        var booking = new Booking
        {
            ApartmentId = request.ApartmentId,
            UserId = userId,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var days = (endDate.Date - startDate.Date).Days;
        var totalPrice = days * apartment.PricePerDay;

        return (booking, days, totalPrice);
    }

    public async Task<List<Booking>> GetAllAsync()
    {
        return await _context.Bookings
            .Include(b => b.Apartment)
            .Include(b => b.User)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetByUserAsync(long userId)
    {
        return await _context.Bookings
            .Where(b => b.UserId == userId)
            .Include(b => b.Apartment)
                .ThenInclude(a => a!.City)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetByApartmentAsync(int apartmentId)
    {
        return await _context.Bookings
            .Where(b => b.ApartmentId == apartmentId && b.EndDate >= DateTime.Today)
            .Include(b => b.User)
            .OrderBy(b => b.StartDate)
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(int id, long userId)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null || booking.UserId != userId)
        {
            return false;
        }

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();
        return true;
    }
}
