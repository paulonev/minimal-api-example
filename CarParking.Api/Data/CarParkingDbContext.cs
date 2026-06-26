using CarParking.Api.Data.Entities;
using CarParking.Core.Rules;
using Microsoft.EntityFrameworkCore;

namespace CarParking.Api.Data;

public class CarParkingDbContext : DbContext
{
    public DbSet<ParkingSession> ParkingSessions => Set<ParkingSession>();
    public DbSet<ParkingSpace> ParkingSpaces => Set<ParkingSpace>();

    public CarParkingDbContext(DbContextOptions<CarParkingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ParkingSpace>(entity =>
        {
            entity.ToTable("parking_spaces");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.SpaceNumber)
                .IsRequired();

            entity.Property(x => x.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);

            entity.HasIndex(x => x.SpaceNumber)
                .IsUnique();
        });

        modelBuilder.Entity<ParkingSession>(entity =>
        {
            entity.ToTable("parking_sessions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.VehicleType)
                .IsRequired();

            entity.Property(x => x.VehicleReg)
                .IsRequired()
                .HasMaxLength(VehicleRegistrationRule.MaxLength);

            entity.Property(x => x.TimeIn)
                .IsRequired()
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.TimeOut)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(false);

            entity.HasOne(x => x.ParkingSpace)
                .WithMany()
                .HasForeignKey("ParkingSpaceId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.HasIndex(x => x.VehicleReg);
            entity.HasIndex("ParkingSpaceId");

            // Allow only one active parking session per vehicle registration.
            entity.HasIndex(x => x.VehicleReg)
                .IsUnique()
                .HasFilter("\"TimeOut\" IS NULL");

            // Allow only one active parking session per parking space.
            entity.HasIndex("ParkingSpaceId")
                .IsUnique()
                .HasFilter("\"TimeOut\" IS NULL");
        });
    }
}