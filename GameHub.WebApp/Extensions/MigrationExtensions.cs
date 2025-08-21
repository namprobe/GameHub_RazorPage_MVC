using Microsoft.EntityFrameworkCore;
using GameHub.DAL.Context;

namespace GameHub.WebApp.Extensions;

public static class MigrationExtensions
{
    /// <summary>
    /// Tự động apply các migration pending cho GameHub database
    /// </summary>
    /// <param name="app">IApplicationBuilder</param>
    /// <param name="logger">ILogger</param>
    /// <returns>Task</returns>
    public static async Task ApplyMigrationsAsync(this IApplicationBuilder app, ILogger logger)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var gameHubContext = scope.ServiceProvider.GetRequiredService<GameHubContext>();

            logger.LogInformation("Starting GameHub database migrations...");
            
            // Kiểm tra kết nối database với retry logic
            try
            {
                await RetryDatabaseConnectionAsync(gameHubContext, "GameHub", logger);
                logger.LogInformation("Successfully connected to GameHub database.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to GameHub database after multiple attempts!");
                throw;
            }

            // Apply pending migrations cho GameHub
            try
            {
                var pendingMigrations = await gameHubContext.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await gameHubContext.Database.GetAppliedMigrationsAsync();

                logger.LogInformation(
                    "GameHub DB: Found {PendingCount} pending migrations and {AppliedCount} previously applied migrations",
                    pendingMigrations.Count(),
                    appliedMigrations.Count());

                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying pending GameHub migrations: {Migrations}", 
                        string.Join(", ", pendingMigrations));
                        
                    await gameHubContext.Database.MigrateAsync();
                    logger.LogInformation("Successfully applied all pending GameHub migrations.");
                }
                else
                {
                    logger.LogInformation("No pending migrations found for GameHub database.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while applying GameHub migrations!");
                throw;
            }

            logger.LogInformation("GameHub database migrations completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "A problem occurred during GameHub database migrations!");
            throw;
        }
    }

    /// <summary>
    /// Thử kết nối database với retry logic
    /// </summary>
    /// <param name="context">DbContext</param>
    /// <param name="contextName">Tên context để log</param>
    /// <param name="logger">ILogger</param>
    /// <param name="maxRetries">Số lần thử tối đa</param>
    /// <param name="delaySeconds">Thời gian delay giữa các lần thử</param>
    /// <returns>Task</returns>
    private static async Task RetryDatabaseConnectionAsync(DbContext context, string contextName, 
        ILogger logger, int maxRetries = 3, int delaySeconds = 5)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Attempting to connect to {ContextName} database (attempt {Attempt}/{MaxRetries})...", 
                    contextName, attempt, maxRetries);
                
                // Test connection
                await context.Database.CanConnectAsync();
                logger.LogInformation("Successfully connected to {ContextName} database on attempt {Attempt}", 
                    contextName, attempt);
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger.LogWarning(ex, 
                    "Failed to connect to {ContextName} database on attempt {Attempt}. Retrying in {DelaySeconds} seconds...", 
                    contextName, attempt, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to {ContextName} database after {MaxRetries} attempts", 
                    contextName, maxRetries);
                throw;
            }
        }
    }

    /// <summary>
    /// Đảm bảo database được tạo nếu chưa tồn tại
    /// </summary>
    /// <param name="app">IApplicationBuilder</param>
    /// <param name="logger">ILogger</param>
    public static void EnsureDatabaseCreated(this IApplicationBuilder app, ILogger logger)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var gameHubContext = scope.ServiceProvider.GetRequiredService<GameHubContext>();

            logger.LogInformation("Checking if GameHub database exists...");
            
            if (gameHubContext.Database.EnsureCreated())
            {
                logger.LogInformation("GameHub database was created successfully.");
            }
            else
            {
                logger.LogInformation("GameHub database already exists.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while ensuring GameHub database exists!");
            throw;
        }
    }
}
