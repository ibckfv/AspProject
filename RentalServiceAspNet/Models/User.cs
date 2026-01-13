namespace RentalServiceAspNet.Models;

public class User
{
    public long Id { get; set; }
    public string? FullName { get; set; }
    public string? Login { get; set; }
    public string? PasswordHash { get; set; }
    public string? Role { get; set; } = "user";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Apartment> OwnedApartments { get; set; } = new List<Apartment>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
