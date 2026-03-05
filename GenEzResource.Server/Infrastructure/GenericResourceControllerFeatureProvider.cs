using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using GenEzResource.Server.DataAccess;

namespace GenEzResource.Server.Infrastructure;

public class GenericResourceControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    private readonly ResourceRegistry _registry;

    public GenericResourceControllerFeatureProvider(ResourceRegistry registry)
    {
        _registry = registry;
    }

    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        foreach (var resourceType in _registry.ResourceTypes)
        {
            var controllerType = typeof(ResourceController<>)
                .MakeGenericType(resourceType)
                .GetTypeInfo();

            feature.Controllers.Add(controllerType);
        }
    }
}
