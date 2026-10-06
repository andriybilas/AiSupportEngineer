using AiSupport.Api.OpenApi;
using AiSupport.Application.DependencyInjection;
using AiSupport.Infrastructure.DataBase.Seeding;
using AiSupport.Infrastructure.DependencyInjection;

namespace AiSupport.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddOperationTransformer<BearerSecurityRequirementOperationTransformer>();
            });

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                await app.SeedDevelopmentDataAsync();

                app.MapOpenApi();

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "AiSupport API v1");
                });
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
