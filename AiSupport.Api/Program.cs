using AiSupport.Application.Services;
using AiSupport.Infrastructure.DependencyInjection;

namespace AiSupport.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddScoped<AppUserService>();
            builder.Services.AddScoped<AppServiceCatalogService>();
            builder.Services.AddScoped<TenantService>();
            builder.Services.AddScoped<SubscriptionService>();
            builder.Services.AddScoped<AuthService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
