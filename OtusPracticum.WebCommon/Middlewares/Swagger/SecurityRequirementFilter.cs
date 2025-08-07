using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OtusPracticum.Middlewares.Swagger
{
    public class SecurityRequirementFilter(string apiRefIds) : IOperationFilter
    {
        public readonly string[] _apiRefIds = apiRefIds.Split(";");

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().Any()
                || context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            {
                return;
            }

            var secReqs = new List<OpenApiSecurityRequirement>();

            foreach (var apiRefId in _apiRefIds)
            {
                var secReq = new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = apiRefId } }] = Array.Empty<string>()
                };
                secReqs.Add(secReq);
            }

            operation.Security = secReqs;
        }
    }
}
