using System;
using GenEzResource.UI.MediatR;
using GenEzResource.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Radzen;

namespace GenEzResource.UI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGenEzResourceUI(this IServiceCollection services, Action<ResourceUIRegistry> configure)
    {
        var registry = new ResourceUIRegistry();
        configure(registry);
        services.AddSingleton(registry);

        services.AddScoped<NotificationService>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(GetAllResourcesHandler<>).Assembly);
        });
        return services;
    }
}
