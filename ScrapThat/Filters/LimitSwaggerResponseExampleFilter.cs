using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ScrapThat.Filters // Replace YourProject with your actual namespace
{
    public class LimitSwaggerResponseExampleFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // This limits the response examples Swagger attempts to generate
            foreach (var response in operation.Responses)
            {
                response.Value.Content.Clear(); // Removes large examples
            }
        }
    }
}

