using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GenEzResource.Models;

namespace GenEzResource.Server.DataAccess;

public class ResourceRegistry
{
    private readonly List<Action<ModelBuilder>> _modelConfigurations = new();
    private readonly List<Action<IServiceCollection>> _serviceRegistrations = new();
    private readonly List<Type> _resourceTypes = new();

    public IReadOnlyList<Type> ResourceTypes => _resourceTypes.AsReadOnly();

    public ResourceRegistry AddResource<TResource>(string tableName) where TResource : ResourceBase
    {
        _resourceTypes.Add(typeof(TResource));

        _modelConfigurations.Add(mb =>
        {
            mb.Entity<Resource<TResource>>(b =>
            {
                b.ToTable(tableName);
                b.Property(x => x.Data).HasColumnType("jsonb");
            });
        });

        _serviceRegistrations.Add(sc =>
        {
            sc.AddScoped<IResourceRepository<TResource>, ResourceRepository<TResource>>();
        });

        return this;
    }

    public ResourceRegistry AddResource<TResource, TServiceInterface, TService>(string tableName)
        where TResource : ResourceBase
        where TServiceInterface : class
        where TService : class, TServiceInterface
    {
        AddResource<TResource>(tableName);

        _serviceRegistrations.Add(sc =>
        {
            sc.AddScoped<TServiceInterface, TService>();
        });

        return this;
    }

    public void ApplyModelConfigurations(ModelBuilder modelBuilder)
    {
        foreach (var config in _modelConfigurations)
            config(modelBuilder);
    }

    internal void ApplyServiceRegistrations(IServiceCollection services)
    {
        foreach (var registration in _serviceRegistrations)
            registration(services);
    }
}

public static class ResourceRegistryExtensions
{
    public static IServiceCollection AddResources(this IServiceCollection services, Action<ResourceRegistry> configure)
    {
        var registry = new ResourceRegistry();
        configure(registry);

        services.AddSingleton(registry);
        registry.ApplyServiceRegistrations(services);

        return services;
    }
}
