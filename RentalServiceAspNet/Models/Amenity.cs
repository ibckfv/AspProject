namespace RentalServiceAspNet.Models;

public class Amenity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ICollection<ApartmentAmenity> ApartmentAmenities { get; set; } = new List<ApartmentAmenity>();
}
