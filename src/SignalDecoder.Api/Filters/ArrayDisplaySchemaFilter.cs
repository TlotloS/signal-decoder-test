using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SignalDecoder.Api.Filters;

/// <summary>
/// Schema filter to display arrays in a more compact horizontal format
/// </summary>
public class ArrayDisplaySchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // For integer arrays, set example to show horizontal display
        if (schema.Type == "array" && schema.Items?.Type == "integer")
        {
            // Set a compact example for integer arrays
            schema.Example = new OpenApiArray
            {
                new OpenApiInteger(2),
                new OpenApiInteger(5),
                new OpenApiInteger(1),
                new OpenApiInteger(3)
            };
        }

        // For properties that are dictionaries with int array values
        if (schema.Type == "object" && schema.AdditionalProperties != null)
        {
            var additionalProps = schema.AdditionalProperties;
            if (additionalProps.Type == "array" && additionalProps.Items?.Type == "integer")
            {
                // Set compact example for dictionary of arrays
                additionalProps.Example = new OpenApiArray
                {
                    new OpenApiInteger(2),
                    new OpenApiInteger(5),
                    new OpenApiInteger(1),
                    new OpenApiInteger(3)
                };
            }
        }
    }
}
