using AiSupport.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AiSupport.Application.DependencyInjection
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<AppUserService>();
            services.AddScoped<AppServiceCatalogService>();
            services.AddScoped<TenantService>();
            services.AddScoped<SubscriptionService>();
            services.AddScoped<AuthService>();

            return services;
        }
    }
}
