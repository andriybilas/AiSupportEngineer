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

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.MapControllers();

            app.Run();
        }
    }
}
