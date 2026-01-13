namespace RentalServiceAspNet.Controllers.RequestEntities;

public class CreateApartmentRequest
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int PricePerDay { get; set; }
    public int? CityId { get; set; }
    public string? ViewType { get; set; }
    public string? Lighting { get; set; }
    public string? Renovation { get; set; }
    public int? Floor { get; set; }
    public int? TotalArea { get; set; }
    public int? Rooms { get; set; }
    public bool Furnished { get; set; }
    public bool PetsAllowed { get; set; }
    public bool Parking { get; set; }
    public bool Internet { get; set; }
}