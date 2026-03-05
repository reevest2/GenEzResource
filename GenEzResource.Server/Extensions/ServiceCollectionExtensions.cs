using Microsoft.Extensions.DependencyInjection;
using GenEzResource.Server.DataAccess;
using GenEzResource.Server.Infrastructure;
using GenEzResource.Server.Services;

namespace GenEzResource.Server.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all ResourceFramework server-side services: resource registry, repositories,
    /// generic resource services, and auto-generated API controllers for each registered resource type.
    ///
    /// Usage:
    /// <code>
    /// builder.Services.AddResourceFramework(registry =>
    /// {
    ///     registry.AddResource&lt;MyResource&gt;("MyResourcesTable");
    /// });
    /// </code>
    /// </summary>
    public static IMvcBuilder AddResourceFramework(this IServiceCollection services, Action<ResourceRegistry> configure)
    {
        // Register the open-generic service so IResourceService<T> resolves for any T
        services.AddScoped(typeof(IResourceService<>), typeof(ResourceService<>));

        // Build the registry with user-defined resources
        services.AddResources(configure);

        // Add controllers with the convention that names them after the resource type
        var mvcBuilder = services.AddControllers(options =>
        {
            options.Conventions.Add(new ResourceControllerModelConvention());
        });

        // Wire up the feature provider that auto-creates ResourceController<T> for each registered type
        mvcBuilder.ConfigureApplicationPartManager(manager =>
        {
            var sp = services.BuildServiceProvider();
            var registry = sp.GetRequiredService<ResourceRegistry>();
            manager.FeatureProviders.Add(new GenericResourceControllerFeatureProvider(registry));
        });

        return mvcBuilder;
    }

    /// <summary>
    /// Overload that also allows configuring MVC options beyond the resource framework defaults.
    /// </summary>
    public static IMvcBuilder AddResourceFramework(
        this IServiceCollection services,
        Action<ResourceRegistry> configureResources,
        Action<Microsoft.AspNetCore.Mvc.MvcOptions> configureMvc)
    {
        services.AddScoped(typeof(IResourceService<>), typeof(ResourceService<>));
        services.AddResources(configureResources);

        var mvcBuilder = services.AddControllers(options =>
        {
            options.Conventions.Add(new ResourceControllerModelConvention());
            configureMvc.Invoke(options);
        });

        mvcBuilder.ConfigureApplicationPartManager(manager =>
        {
            var sp = services.BuildServiceProvider();
            var registry = sp.GetRequiredService<ResourceRegistry>();
            manager.FeatureProviders.Add(new GenericResourceControllerFeatureProvider(registry));
        });

        return mvcBuilder;
    }
}
