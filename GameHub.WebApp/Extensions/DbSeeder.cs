using GameHub.DAL.Common;
using GameHub.DAL.Context;
using GameHub.DAL.Entities;
using GameHub.DAL.Enums;
using GameHub.BLL.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GameHub.WebApp.Extensions;

public static class DbSeeder
{
    /// <summary>
    /// Seed dữ liệu cơ bản cho GameHub database
    /// </summary>
    /// <param name="app">IApplicationBuilder</param>
    /// <param name="logger">ILogger</param>
    /// <returns>Task</returns>
    public static async Task SeedDataAsync(this IApplicationBuilder app, ILogger logger)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<GameHubContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Kiểm tra xem có enable seeding không
            var seedingEnabled = configuration.GetValue<bool>("DataSeeding:EnableSeeding", true);
            if (!seedingEnabled)
            {
                logger.LogInformation("Data seeding is disabled in configuration.");
                return;
            }

            logger.LogInformation("Starting data seeding...");

            // Seed Admin User
            await SeedAdminUserAsync(context, configuration, logger);

            // Save changes
            var changesSaved = await context.SaveChangesAsync();
            if (changesSaved > 0)
            {
                logger.LogInformation("Data seeding completed successfully. {ChangesSaved} changes saved.", changesSaved);
            }
            else
            {
                logger.LogInformation("Data seeding completed. No new data was needed.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during data seeding!");
            throw;
        }
    }

    /// <summary>
    /// Seed Admin User account
    /// </summary>
    /// <param name="context">GameHubContext</param>
    /// <param name="configuration">IConfiguration</param>
    /// <param name="logger">ILogger</param>
    /// <returns>Task</returns>
    private static async Task SeedAdminUserAsync(GameHubContext context, IConfiguration configuration, ILogger logger)
    {
        // Đọc admin config từ appsettings
        var adminEmail = configuration.GetValue<string>("AdminUser:Email", "admin@gamehub.com");
        var adminPassword = configuration.GetValue<string>("AdminUser:DefaultPassword", "Admin@123");
        var adminUsername = configuration.GetValue<string>("AdminUser:Username", "Administrator");

        // Kiểm tra xem đã có admin chưa
        var existingAdmin = await context.Users
            .FirstOrDefaultAsync(u => u.Role == RoleEnum.Admin);

        if (existingAdmin != null)
        {
            logger.LogInformation("Admin user already exists: {Email}", existingAdmin.Email);
            return;
        }

        // Tạo admin user
        var adminUser = new User
        {
            Email = adminEmail!,
            PasswordHash = PasswordHelper.HashPassword(adminPassword!),
            Role = RoleEnum.Admin,
            IsActive = true,
            JoinDate = DateTime.Now
        };
        BaseEntityExtension.InitializeAudit(adminUser);
        context.Users.Add(adminUser);
        logger.LogInformation("Created admin user: {Email} with default password: {Password}", 
            adminUser.Email, adminPassword);

        // Tạo Player record cho admin (optional - để có thể login với Player role nếu cần)
        var adminPlayer = new Player
        {
            Username = adminUsername!,
            User = adminUser,
            LastLogin = null
        };

        context.Players.Add(adminPlayer);
        logger.LogInformation("Created admin player profile: {Username}", adminPlayer.Username);

        // Log credentials cho development
        logger.LogWarning("🔐 ADMIN CREDENTIALS - Email: {Email} | Password: {Password} ", 
            adminEmail, adminPassword);
        logger.LogWarning("⚠️  Please change the default password after first login!");
    }
}
