using RentalServiceAspNet.Controllers.RequestEntities;
using RentalServiceAspNet.Models;

namespace RentalServiceAspNet.Services;

public interface IBookingService
{
    Task<(Booking booking, int days, int totalPrice)> CreateAsync(CreateBookingRequest request, long userId);
    Task<List<Booking>> GetAllAsync();
    Task<List<Booking>> GetByUserAsync(long userId);
    Task<List<Booking>> GetByApartmentAsync(int apartmentId);
    Task<bool> DeleteAsync(int id, long userId);
}