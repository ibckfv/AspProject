namespace RentalServiceAspNet.Controllers.RequestEntities;

public class CreateBookingRequest
{
    public int ApartmentId { get; set; }
    public string StartDate { get; set; } = "";
    public string EndDate { get; set; } = "";
}