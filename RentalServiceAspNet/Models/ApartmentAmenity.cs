namespace RentalServiceAspNet.Models;

public class ApartmentAmenity
{
    public int ApartmentId { get; set; }
    public int AmenityId { get; set; }
    public Apartment Apartment { get; set; } = null!;
    public Amenity Amenity { get; set; } = null!;
}
