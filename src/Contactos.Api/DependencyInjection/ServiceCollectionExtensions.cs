using Contactos.Api.Repositories;
using Contactos.Api.Services.Commands;
using Contactos.Api.Services.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Contactos.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IContactRepository, ContactRepository>();
        services.AddScoped<IContactQueries, ContactQueries>();
        services.AddScoped<IContactCommands, ContactCommands>();
        return services;
    }
}
