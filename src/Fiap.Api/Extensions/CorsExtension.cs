namespace Fiap.Api.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class CorsExtensions
    {
        public static void AddGlobalCorsPolicy(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyHeader()
                           .AllowAnyMethod();
                });
            });
        }
    }

}
