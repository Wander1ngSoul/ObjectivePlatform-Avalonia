using Microsoft.EntityFrameworkCore;
using ObjectivePlatformApp.Models;

namespace ObjectivePlatformApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<Clients> Clients => Set<Clients>();
    public DbSet<Agents> Agents => Set<Agents>();
    public DbSet<RealEstateType> RealEstateTypes => Set<RealEstateType>();
    public DbSet<Districts> Districts => Set<Districts>();
    public DbSet<RealEstates> RealEstates => Set<RealEstates>();
    public DbSet<Offers> Offers => Set<Offers>();
    public DbSet<Demands> Demands => Set<Demands>();
    public DbSet<HouseDemands> HouseDemands => Set<HouseDemands>();
    public DbSet<FlatDemands> FlatDemands => Set<FlatDemands>();
    public DbSet<LandDemands> LandDemands => Set<LandDemands>();
    public DbSet<Deal> Deals => Set<Deal>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=Huawei_D_15;Database=ObjectivePlatform;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Clients>()
            .HasCheckConstraint("CK_Clients_EmailOrPhone", "Email IS NOT NULL OR Phone IS NOT NULL");

        modelBuilder.Entity<Clients>()
            .HasIndex(c => c.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        modelBuilder.Entity<Clients>()
            .HasIndex(c => c.Phone)
            .IsUnique()
            .HasFilter("[Phone] IS NOT NULL");

        modelBuilder.Entity<Deal>()
            .HasOne(d => d.Demand)
            .WithOne(d => d.Deal)
            .HasForeignKey<Deal>(d => d.DemandId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Deal>()
            .HasOne(d => d.Offer)
            .WithOne(o => o.Deal)
            .HasForeignKey<Deal>(d => d.OfferId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Demands>()
            .HasOne(d => d.Deal)
            .WithOne(d => d.Demand)
            .HasForeignKey<Deal>(d => d.DemandId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Offers>()
            .HasOne(o => o.Deal)
            .WithOne(d => d.Offer)
            .HasForeignKey<Deal>(d => d.OfferId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Agents>()
            .Property(a => a.Commision)
            .HasAnnotation("CheckConstraint", "Commision > 0 AND Commision < 100");

        modelBuilder.Entity<RealEstates>()
            .Property(r => r.Latitude)
            .HasAnnotation("CheckConstraint", "Latitude >= -90 AND Latitude <= 90");

        modelBuilder.Entity<RealEstates>()
            .Property(r => r.Longitude)
            .HasAnnotation("CheckConstraint", "Longitude >= -180 AND Longitude <= 180");

        modelBuilder.Entity<Offers>()
            .Property(o => o.Price)
            .HasAnnotation("CheckConstraint", "Price > 0");

        modelBuilder.Entity<Demands>()
            .Property(d => d.MinPrice)
            .HasAnnotation("CheckConstraint", "MinPrice > 0");

        modelBuilder.Entity<Demands>()
            .Property(d => d.MaxPrice)
            .HasAnnotation("CheckConstraint", "MaxPrice > 0");
    }
}