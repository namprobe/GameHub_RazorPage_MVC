using GameHub.WebApp.Extensions;

namespace GameHub.WebApp.Configurations;

public static class SeedConfiguration
{
    
    public static async Task ConfigureSeedingAsync(this IApplicationBuilder app, IWebHostEnvironment environment, ILogger logger)
    {
        try
        {
            logger.LogInformation("Configuring admin user seeding for environment: {Environment}", environment.EnvironmentName);

            // Chỉ seed admin user trong mọi môi trường
            await app.SeedDataAsync(logger);

            logger.LogInformation("Admin user seeding configuration completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during admin user seeding!");
            
            // Trong production không crash app vì seed data
            if (environment.IsProduction())
            {
                logger.LogWarning("Continuing application startup despite seeding errors (Production mode).");
            }
            else
            {
                throw;
            }
        }
    }

    
    public static async Task EnsureAdminUserAsync(this IApplicationBuilder app, ILogger logger)
    {
        try
        {
            logger.LogInformation("Ensuring admin user exists...");
            await app.SeedDataAsync(logger);
            logger.LogInformation("Admin user verification completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure admin user exists!");
            throw;
        }
    }
}
