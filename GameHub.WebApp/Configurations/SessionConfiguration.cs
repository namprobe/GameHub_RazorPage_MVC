namespace GameHub.Configurations
{
    public static class SessionConfiguration
    {
        public static IServiceCollection AddSessionConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var sessionSettings = configuration.GetSection("Session");
            
            // Cấu hình MemoryCache cho Session (required)
            services.AddDistributedMemoryCache();
                
            // Cấu hình Session đơn giản
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(
                    sessionSettings.GetValue<int>("IdleTimeout", 150));
                options.Cookie.Name = sessionSettings.GetValue<string>("CookieName", "GameHub.Session");
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Cấu hình HttpContextAccessor
            services.AddHttpContextAccessor();

            return services;
        }
    }

}
