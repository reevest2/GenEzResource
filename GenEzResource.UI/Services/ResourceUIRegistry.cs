using GenEzResource.Models;

namespace GenEzResource.UI.Services;

public class ResourceUIRegistration
{
    public Type ResourceType { get; set; } = null!;
    public string RouteName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class ResourceUIRegistry
{
    private readonly Dictionary<Type, ResourceUIRegistration> _registrations = new();
    private readonly Dictionary<string, ResourceUIRegistration> _registrationsByRoute = new(StringComparer.OrdinalIgnoreCase);

    public ResourceUIRegistry AddResource<T>(string? routeName = null, string? displayName = null) where T : ResourceBase
    {
        var type = typeof(T);
        var route = routeName ?? type.Name;
        var display = displayName ?? type.Name;

        var registration = new ResourceUIRegistration
        {
            ResourceType = type,
            RouteName = route,
            DisplayName = display
        };

        _registrations[type] = registration;
        _registrationsByRoute[route] = registration;
        return this;
    }

    public IReadOnlyCollection<ResourceUIRegistration> GetAll() => _registrations.Values;

    public string GetRouteName<T>() where T : ResourceBase => GetRouteName(typeof(T));

    public string GetRouteName(Type type)
    {
        return _registrations.TryGetValue(type, out var reg) ? reg.RouteName : type.Name;
    }

    public Type? GetResourceType(string routeName)
    {
        return _registrationsByRoute.TryGetValue(routeName, out var reg) ? reg.ResourceType : null;
    }
}
