using HotelRoomBookingAPI.Data;
using HotelRoomBookingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelRoomBookingAPI.Services;

public class AdminSeederService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminSeederService> _logger;

    public AdminSeederService(ApplicationDbContext context, ILogger<AdminSeederService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAdminAsync()
    {
        try
        {
            // Check if any admin user exists (RoleId = 1)
            var adminExists = await _context.Users.AnyAsync(u => u.RoleId == 1);

            if (!adminExists)
            {
                _logger.LogInformation("No admin user found. Creating default admin account...");

                // Create default admin account
                var admin = new User
                {
                    FullName = "System Administrator",
                    Email = "admin@hotelmanagement.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    RoleId = 1,
                    CompanyName = "Hotel Management System"
                };

                _context.Users.Add(admin);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Default admin account created successfully.");
                _logger.LogInformation("Email: admin@hotelmanagement.com | Password: Admin@123");
                _logger.LogWarning("IMPORTANT: Please change the default admin password after first login!");
            }
            else
            {
                _logger.LogInformation("Admin user already exists. Skipping admin seeding.");
            }

            // Seed Client Kinds
            if (!await _context.ClientKinds.AnyAsync())
            {
                _logger.LogInformation("Seeding Client Kinds...");
                var clientKinds = new List<ClientKind>
                {
                    new ClientKind { ClientKindName = "Employee", Description = "Internal company employee" },
                    new ClientKind { ClientKindName = "Staff", Description = "Hotel staff member" },
                    new ClientKind { ClientKindName = "Guest", Description = "External guest" },
                    new ClientKind { ClientKindName = "Player", Description = "Sports player or team member" }
                };

                _context.ClientKinds.AddRange(clientKinds);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Client Kinds seeded successfully.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding admin user.");
            throw;
        }
    }
}
