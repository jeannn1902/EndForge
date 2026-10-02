using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EndForge.Data;
using EndForge.Repositories;

namespace EndForge.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
        {
            // DbContext OnConfiguring está preparado para usar la cadena embebida o la proporcionada externamente.
            // services.AddDbContext<EndForgeDbContext>();

            services.AddScoped<IProgresoRepository, ProgresoRepository>();

            // Registrar logger factory por si se requiere inyección
            services.AddLogging();

            return services;
        }
    }
}
