using RentalServiceAspNet.Controllers.RequestEntities;
using RentalServiceAspNet.Models;

namespace RentalServiceAspNet.Services;

public interface IApartmentService
{
    Task<List<Apartment>> GetAllAsync();
    Task<Apartment?> GetByIdAsync(int id);
    Task<List<Apartment>> GetByOwnerAsync(long ownerId);
    Task<Apartment> CreateAsync(CreateApartmentRequest request, long ownerId);
}