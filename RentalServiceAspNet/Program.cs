using Microsoft.EntityFrameworkCore;
using RentalServiceAspNet.Data;
using RentalServiceAspNet.Services;
using RentalServiceAspNet.Middleware;
using RentalServiceAspNet.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IApartmentService, ApartmentService>();
builder.Services.AddScoped<IBookingService, BookingService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddSingleton<ILoggerProvider>(sp => new DatabaseLoggerProvider(sp));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseMiddleware<JwtCookieMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "apartments-index",
    pattern: "apartments",
    defaults: new { controller = "Apartments", action = "Index" });

app.MapControllerRoute(
    name: "apartments-add",
    pattern: "apartments/add",
    defaults: new { controller = "Apartments", action = "Add" });

app.MapControllerRoute(
    name: "apartments-my",
    pattern: "apartments/my",
    defaults: new { controller = "Apartments", action = "My" });

app.MapControllerRoute(
    name: "apartments-details",
    pattern: "apartments/{id:int}",
    defaults: new { controller = "Apartments", action = "Details" });

app.MapControllerRoute(
    name: "bookings-my",
    pattern: "bookings/my",
    defaults: new { controller = "Bookings", action = "My" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
