namespace Microservicio.Atracciones.Api.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddRoleAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("SoloAdmin", p => p.RequireRole("ADMIN"));
                options.AddPolicy("AdminOOperador", p => p.RequireRole("ADMIN", "OPERADOR"));
                options.AddPolicy("Todos", p => p.RequireRole("ADMIN", "OPERADOR", "VENDEDOR"));
            });

            return services;
        }
    }
}
