using GameServer.Database.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace GameServer.Modules.Auth
{
    public static class ServiceCollectionExtensionsAuth
    {
        public static IServiceCollection AddAuthModule(this IServiceCollection services)
        {
            services.AddSingleton<PlayerRepository>();

            return services;
        }
    }
}
