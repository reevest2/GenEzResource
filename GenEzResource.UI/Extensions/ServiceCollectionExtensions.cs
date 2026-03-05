using System;
using GenEzResource.UI.MediatR;
using GenEzResource.UI.Services;
using MediatR;
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

        // Register open generic handlers for MediatR
        services.AddTransient(typeof(IRequestHandler<,>), typeof(GetAllResourcesHandler<>));
        services.AddTransient(typeof(IRequestHandler<,>), typeof(GetResourceByIdHandler<>));
        services.AddTransient(typeof(IRequestHandler<,>), typeof(GetResourcesByFilterHandler<>));
        services.AddTransient(typeof(IRequestHandler<,>), typeof(UpsertResourceHandler<>));
        services.AddTransient(typeof(IRequestHandler<,>), typeof(SoftDeleteResourceHandler<>));

        return services;
    }
}
