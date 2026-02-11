using Microsoft.EntityFrameworkCore;
using HotelRoomBookingAPI.Models;

namespace HotelRoomBookingAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<BuildingsMaster> BuildingsMasters { get; set; }
    public DbSet<Floor> Floors { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<RoomStatus> RoomStatuses { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<BookingOccupant> BookingOccupants { get; set; }
    public DbSet<ClientKind> ClientKinds { get; set; }
    public DbSet<OccupantDailyMeal> OccupantDailyMeals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map entities 

        // Configure BuildingsMaster entity -> buildings_master table
        modelBuilder.Entity<BuildingsMaster>(entity =>
        {
            entity.ToTable("buildings_master"); // Explicit table name
            entity.HasKey(e => e.Building_ID);
            entity.Property(e => e.Building_Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Building_Location).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20);
        });

        // Configure Floor entity -> Floors table
        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasKey(e => e.FloorId);
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.No_of_rooms).IsRequired(); // New column

            entity.HasOne(e => e.BuildingsMaster)
                .WithMany() 
                .HasForeignKey(e => e.Building_ID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure RoomType entity -> RoomTypes table
        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasKey(e => e.RoomTypeId);
            entity.Property(e => e.RoomTypeName).IsRequired().HasMaxLength(50);
        });

        // Configure Room entity -> Rooms table
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId);
            entity.Property(e => e.RoomNumber).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(e => e.Building)
                .WithMany(h => h.Rooms)
                .HasForeignKey(e => e.Building_ID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Floor)
                .WithMany(f => f.Rooms)
                .HasForeignKey(e => e.FloorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RoomType)
                .WithMany(rt => rt.Rooms)
                .HasForeignKey(e => e.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Role entity -> Roles table
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
        });

        // Configure User entity -> Users table
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CompanyName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Booking entity -> Bookings table
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId);
            entity.Property(e => e.BookingStatus).IsRequired().HasMaxLength(20);

            entity.HasOne(e => e.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure BookingOccupant entity -> BookingOccupants table
        modelBuilder.Entity<BookingOccupant>(entity =>
        {
            entity.HasKey(e => e.BookingOccupantId);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(15);
            entity.Property(e => e.AadhaarLast4).IsRequired().HasMaxLength(4);
            
            // Unique Index on AadhaarHash to prevent duplicate entries
            entity.HasIndex(e => e.AadhaarHash)
                  .IsUnique()
                  .HasDatabaseName("UX_BookingOccupants_AadhaarHash");

            entity.HasOne(e => e.Booking)
                .WithMany(b => b.Occupants)
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure OccupantDailyMeal entity -> OccupantDailyMeals table
        modelBuilder.Entity<OccupantDailyMeal>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.BookingOccupant)
                .WithMany(o => o.DailyMeals)
                .HasForeignKey(e => e.BookingOccupantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
