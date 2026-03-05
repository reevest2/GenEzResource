using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace GenEzResource.Server.Infrastructure;

public class ResourceControllerModelConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (!controller.ControllerType.IsGenericType ||
            controller.ControllerType.GetGenericTypeDefinition() != typeof(ResourceController<>))
            return;

        var resourceType = controller.ControllerType.GenericTypeArguments[0];
        controller.ControllerName = resourceType.Name;
    }
}
