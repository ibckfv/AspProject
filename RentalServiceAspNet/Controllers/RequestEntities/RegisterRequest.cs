namespace RentalServiceAspNet.Controllers.RequestEntities;

public class RegisterRequest
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public string FullName { get; set; } = "";
}