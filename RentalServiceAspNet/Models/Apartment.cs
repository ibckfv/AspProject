namespace RentalServiceAspNet.Models;

public class Apartment
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int PricePerDay { get; set; }
    public int? CityId { get; set; }
    public long? OwnerId { get; set; }
    public string? ViewType { get; set; }
    public string? Location { get; set; }
    public string? Lighting { get; set; }
    public string? Renovation { get; set; }
    public int? Floor { get; set; }
    public int? TotalArea { get; set; }
    public int? Rooms { get; set; }
    public bool Furnished { get; set; }
    public bool PetsAllowed { get; set; }
    public bool Parking { get; set; }
    public bool Internet { get; set; }
    public string? Status { get; set; }
    public string? MainPhoto { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public City? City { get; set; }
    public User? Owner { get; set; }
    public ICollection<ApartmentAmenity> ApartmentAmenities { get; set; } = new List<ApartmentAmenity>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
