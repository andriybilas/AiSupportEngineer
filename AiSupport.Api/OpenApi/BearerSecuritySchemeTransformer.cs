using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AiSupport.Api.OpenApi
{
    internal sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        public const string SchemeName = "Bearer";

        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT access token returned by POST /api/Auth/login or /api/Auth/register."
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[SchemeName] = securityScheme;

            return Task.CompletedTask;
        }
    }
}
