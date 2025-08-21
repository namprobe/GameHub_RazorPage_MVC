using GameHub.DAL.Interfaces;
using GameHub.DAL.Implements;
using GameHub.DAL.Context;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using GameHub.Configurations;
using GameHub.BLL.Helpers;
using GameHub.BLL.Mapping;
using GameHub.BLL.QueryBuilders;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Implements;
using GameHub.BLL.Hubs;

namespace GameHub.WebApp.Configurations;
public static class ServiceConfiguration
{
    public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // Cấu hình SQL Server Database Context
        services.AddDbContext<GameHubContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            
            // Tắt toàn bộ logging của EF Core
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
            options.EnableServiceProviderCaching(true);
            options.ConfigureWarnings(warnings => 
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.FirstWithoutOrderByAndFilterWarning));
        });

        return services;
    }

    public static IServiceCollection ConfigureAutoMapper(this IServiceCollection services)
    {
        var mapperConfig = new MapperConfiguration(mc =>
        {
            // Đăng ký tất cả mapping profiles
            mc.AddProfile(new GameCategoryMappingProfile());
            mc.AddProfile(new DeveloperMappingProfile());
            mc.AddProfile(new GameMappingProfile());
        });
        IMapper mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);
        return services;
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Repository Pattern với Filter Model
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Add các Repository
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IGameCategoryRepository, GameCategoryRepository>();
        services.AddScoped<IDeveloperRepository, DeveloperRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IGameRegistrationRepository, GameRegistrationRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartItemRepository, CartItemRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IGameRegistrationDetailRepository, GameRegistrationDetailRepository>();

        //Add UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add Helpers
        services.AddScoped<SessionHelper>();
        services.AddScoped<CurrentUserHelper>();
        services.AddScoped<FileUploadHelper>();

        // Add Query Builders
        services.AddScoped<GameCategoryQueryBuilder>();
        services.AddScoped<GameQueryBuilder>();
        services.AddScoped<DeveloperQueryBuilder>();

        // Add các Service cụ thể
        services.AddScoped<IGameCategoryService, GameCategoryService>();
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<IDeveloperService, DeveloperService>();
        services.AddScoped<IAuthService, AuthService>();
        
        return services;
    }

    // ✨ Method riêng cho SignalR configuration
    public static IServiceCollection ConfigureSignalR(this IServiceCollection services)
    {
        services.AddSignalR(options =>
        {
            // Cấu hình cho development
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    // ✨ Method để map SignalR hubs 
    public static WebApplication MapSignalRHubs(this WebApplication app)
    {
        
        app.MapHub<GameHub.BLL.Hubs.GameHub>("/gameHub");
        
        return app;
    }

    public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Cấu hình Logging (tắt EF Core logs)
        services.ConfigureLogging();
        
        // Cấu hình Database
        services.ConfigureDatabase(configuration);

        // Cấu hình AutoMapper
        services.ConfigureAutoMapper();

        // Cấu hình JWT Authentication
        services.AddJwtAuthentication(configuration);

        // Cấu hình Session
        services.AddSessionConfiguration(configuration);

        // Cấu hình các Service
        services.ConfigureServices();

        // Cấu hình SignalR
        services.ConfigureSignalR();

        return services;
    }
}
