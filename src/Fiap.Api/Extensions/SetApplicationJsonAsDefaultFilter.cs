namespace Fiap.Api.Extensions
{
    [ExcludeFromCodeCoverage]
    public class SetApplicationJsonAsDefaultFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.RequestBody?.Content != null &&
                operation.RequestBody.Content.ContainsKey("application/json"))
            {
                var content = operation.RequestBody.Content;
                var jsonMediaType = content["application/json"];
                operation.RequestBody.Content.Remove("application/json");

                var reordered = new Dictionary<string, OpenApiMediaType>
            {
                { "application/json", jsonMediaType }
            };

                foreach (var kvp in content)
                    reordered[kvp.Key] = kvp.Value;

                operation.RequestBody.Content = reordered;
            }

            foreach (var response in operation.Responses)
            {
                if (response.Value.Content != null &&
                    response.Value.Content.ContainsKey("application/json"))
                {
                    var content = response.Value.Content;
                    var jsonMediaType = content["application/json"];
                    content.Remove("application/json");

                    var reordered = new Dictionary<string, OpenApiMediaType>
                {
                    { "application/json", jsonMediaType }
                };

                    foreach (var kvp in content)
                        reordered[kvp.Key] = kvp.Value;

                    response.Value.Content = reordered;
                }
            }
        }
    }
}
