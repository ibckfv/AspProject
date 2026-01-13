using Microsoft.EntityFrameworkCore;
using RentalServiceAspNet.Models;

namespace RentalServiceAspNet.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Apartment> Apartments { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<ApartmentAmenity> ApartmentAmenities { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<LogEntry> LogEntries { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.HasIndex(e => e.Login).IsUnique();
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<Apartment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.HasOne(e => e.City)
                .WithMany(c => c.Apartments)
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.OwnedApartments)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Amenity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<ApartmentAmenity>(entity =>
        {
            entity.HasKey(e => new { e.ApartmentId, e.AmenityId });
            entity.HasOne(e => e.Apartment)
                .WithMany(a => a.ApartmentAmenities)
                .HasForeignKey(e => e.ApartmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Amenity)
                .WithMany(a => a.ApartmentAmenities)
                .HasForeignKey(e => e.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.HasOne(e => e.Apartment)
                .WithMany(a => a.Bookings)
                .HasForeignKey(e => e.ApartmentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => e.Category);
        });
    }
}
