using Microsoft.Extensions.Logging;

namespace GameHub.WebApp.Configurations;

public static class LoggingConfiguration
{
    /// <summary>
    /// Cấu hình logging để tắt EF Core logs
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection ConfigureLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            // Tắt toàn bộ EF Core logging
            builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);
            builder.AddFilter("Microsoft.EntityFrameworkCore.Database", LogLevel.Error);
            builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Error);
            builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Connection", LogLevel.Error);
            builder.AddFilter("Microsoft.EntityFrameworkCore.Infrastructure", LogLevel.Error);
            builder.AddFilter("Microsoft.EntityFrameworkCore.Migrations", LogLevel.Error);
            builder.AddFilter("Microsoft.EntityFrameworkCore.Model", LogLevel.Error);
            builder.AddFilter("Microsoft.EntityFrameworkCore.Query", LogLevel.Error);
            builder.AddFilter("Microsoft.EntityFrameworkCore.Update", LogLevel.Error);
            
            // Chỉ cho phép logs từ GameHub application
            builder.AddFilter("GameHub", LogLevel.Information);
            builder.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
        });

        return services;
    }

    /// <summary>
    /// Cấu hình logging cho Production - chỉ Error và Critical
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection ConfigureProductionLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            // Production: chỉ Error và Critical
            builder.SetMinimumLevel(LogLevel.Error);
            
            // Tắt hoàn toàn EF Core logs
            builder.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
            
            // Chỉ cho phép Error và Critical từ application
            builder.AddFilter("GameHub", LogLevel.Error);
            builder.AddFilter("Microsoft.AspNetCore", LogLevel.Error);
        });

        return services;
    }
}
