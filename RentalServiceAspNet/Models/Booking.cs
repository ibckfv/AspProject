namespace RentalServiceAspNet.Models;

public class Booking
{
    public int Id { get; set; }
    public int ApartmentId { get; set; }
    public long UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Apartment Apartment { get; set; } = null!;
    public User User { get; set; } = null!;
}
