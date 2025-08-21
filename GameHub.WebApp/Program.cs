using GameHub.WebApp.Configurations;
using GameHub.WebApp.Extensions;

namespace GameHub.WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            
            // Cấu hình GameHub services (JWT, Session, Auth)
            builder.Services.ConfigureApplicationServices(builder.Configuration);
            
            // Cấu hình logging riêng cho Production (tùy chọn - override appsettings)
            if (!builder.Environment.IsDevelopment())
            {
                builder.Services.ConfigureProductionLogging();
            }

            var app = builder.Build();
            
            // Tạo logger để sử dụng cho migrations và seeding
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            
            try
            {
                // Tự động apply migrations khi khởi động
                await app.ApplyMigrationsAsync(logger);
                
                // Seed admin user
                await app.EnsureAdminUserAsync(logger);
                
                logger.LogInformation("Application started successfully with migrations applied and admin user seeded.");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Application failed to start due to migration or seeding errors!");
                
                // Trong môi trường development, có thể muốn tiếp tục chạy
                if (!app.Environment.IsDevelopment())
                {
                    throw;
                }
                
                logger.LogWarning("Continuing application startup despite migration/seeding errors (Development mode).");
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            
            // Session middleware
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            // Map SignalR Hubs
            app.MapSignalRHubs();

            await app.RunAsync();
        }
    }
}
