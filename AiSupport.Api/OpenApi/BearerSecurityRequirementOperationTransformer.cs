using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AiSupport.Api.OpenApi
{
    internal sealed class BearerSecurityRequirementOperationTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(
            OpenApiOperation operation,
            OpenApiOperationTransformerContext context,
            CancellationToken cancellationToken)
        {
            var endpointMetadata = context.Description.ActionDescriptor.EndpointMetadata;

            if (!RequiresAuthorization(endpointMetadata))
            {
                return Task.CompletedTask;
            }

            var schemeReference = new OpenApiSecuritySchemeReference(
                BearerSecuritySchemeTransformer.SchemeName,
                context.Document);

            var requirement = new OpenApiSecurityRequirement
            {
                [schemeReference] = new List<string>()
            };

            operation.Security ??= new List<OpenApiSecurityRequirement>();
            operation.Security.Add(requirement);

            return Task.CompletedTask;
        }

        private static bool RequiresAuthorization(IList<object> endpointMetadata)
        {
            var hasAuthorize = endpointMetadata
                .OfType<IAuthorizeData>()
                .Any();

            var allowsAnonymous = endpointMetadata
                .OfType<IAllowAnonymous>()
                .Any();

            return hasAuthorize && !allowsAnonymous;
        }
    }
}
