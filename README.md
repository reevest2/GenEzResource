# DemonsAndDogs — Generic Resource Framework

A generic, convention-based resource framework built on ASP.NET Core 8, Entity Framework Core, and Blazor WebAssembly. Define a model, register it in one line, and get a full set of CRUD API endpoints **and** UI components automatically.

---

## NuGet Packages

The framework is distributed as two NuGet packages:

| Package | Description | Install In |
|---|---|---|
| **ResourceFramework.Server** | All-in-one server package — includes `ResourceBase`, `Resource<T>`, EF Core repository, CRUD services, generic API controllers, `ResourceRegistry`, `ResourceDbContext`, and one-line DI setup | ASP.NET Core Web API projects |
| **ResourceFramework.UI** | Blazor WASM components — Radzen DataGrid with CRUD, MediatR handlers, `ResourceUIRegistry` | Blazor WebAssembly projects |

Both packages share **ResourceFramework.Models** (included automatically) which contains the `ResourceBase` and `Resource<T>` base classes.

### Package Dependency Graph

```
ResourceFramework.Models          (standalone, no dependencies)
       │
       ├── ResourceFramework.Server  (+ EF Core, ASP.NET Core MVC)
       │
       └── ResourceFramework.UI     (+ Radzen.Blazor, MediatR)
```

---

## Quick Start — Server (API)

### 1. Install the package

```bash
dotnet add package ResourceFramework.Server
```

Or reference the project directly:

```xml
<ProjectReference Include="..\ResourceFramework.Server\ResourceFramework.Server.csproj" />
```

### 2. Define a resource model

```csharp
using ResourceFramework.Models;

namespace MyApp;

public class TodoItem : ResourceBase
{
    public string Title { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
}
```

### 3. Register it in `Program.cs` (one line!)

```csharp
using ResourceFramework.Server.Extensions;
using ResourceFramework.Server.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// One call registers: repository, service, EF table mapping, and API controller
builder.Services.AddResourceFramework(registry =>
{
    registry.AddResource<TodoItem>("TodoItems");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure your DbContext (PostgreSQL example using ResourceDbContext)
builder.Services.AddDbContext<ResourceDbContext>(options =>
    options.UseNpgsql(dataSource));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
```

**That's it!** You now have these endpoints:

| Method | Route | Description |
|---|---|---|
| `GET` | `api/TodoItem/{id}` | Get by ID |
| `GET` | `api/TodoItem/filter?key1=...&key2=...&key3=...` | Filtered list |
| `GET` | `api/TodoItem` | Get all (non-deleted) |
| `POST` | `api/TodoItem` | Upsert (create or update) |
| `DELETE` | `api/TodoItem/{id}` | Soft-delete |

---

## Quick Start — Client (Blazor WASM)

### 1. Install the package

```bash
dotnet add package ResourceFramework.UI
```

### 2. Register resources in `Program.cs`

```csharp
using UI.Component.Extensions;

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddResourceUI(registry =>
{
    registry.AddResource<TodoItem>();
});
```

### 3. Add a resource page

```razor
@page "/resources/{ResourceName}"
@using UI.Component.Components
@using UI.Component.Services
@inject ResourceUIRegistry Registry

@if (_resourceType != null)
{
    <RadzenNotification />
    @_gridFragment
}

@code {
    [Parameter] public string ResourceName { get; set; } = string.Empty;
    private Type? _resourceType;
    private RenderFragment? _gridFragment;

    protected override void OnParametersSet()
    {
        _resourceType = Registry.GetResourceType(ResourceName);
        if (_resourceType != null)
        {
            var gridType = typeof(ResourceGrid<>).MakeGenericType(_resourceType);
            _gridFragment = builder =>
            {
                builder.OpenComponent(0, gridType);
                builder.AddAttribute(1, "Title", ResourceName);
                builder.CloseComponent();
            };
        }
    }
}
```

### 4. Add navigation links

In your `NavMenu.razor`:

```razor
<ResourceNavLinks />
```

This auto-generates sidebar links for every registered resource.

---

## What's Inside ResourceFramework.Server

The server package is fully self-contained. Here's what it provides:

### Models (`ResourceFramework.Models` namespace)

| Class | Description |
|---|---|
| `ResourceBase` | Base class with `Id`, `OwnerId`, `Key1`–`Key3`, `Version`, `CreatedAt`, `UpdatedAt`, `IsDeleted` |
| `Resource<T>` | Wrapper that stores `T Data` as a JSONB column |

### DataAccess (`ResourceFramework.Server.DataAccess` namespace)

| Class / Interface | Description |
|---|---|
| `IResourceRepository<T>` | Repository interface with key-based queries, CRUD, and soft-delete |
| `ResourceRepository<T>` | EF Core implementation of the repository |
| `ResourceRegistry` | Fluent registry for adding resources (configures EF model + DI) |
| `ResourceDbContext` | Base DbContext that auto-applies resource model configurations |

### Services (`ResourceFramework.Server.Services` namespace)

| Class / Interface | Description |
|---|---|
| `IResourceService<T>` | Service interface for business logic layer |
| `ResourceService<T>` | Default implementation delegating to repository |

### Infrastructure (`ResourceFramework.Server.Infrastructure` namespace)

| Class | Description |
|---|---|
| `ResourceController<T>` | Generic API controller with 5 CRUD endpoints |
| `GenericResourceControllerFeatureProvider` | Auto-registers controllers for each resource type |
| `ResourceControllerModelConvention` | Names controllers after the resource type |

### Extensions (`ResourceFramework.Server.Extensions` namespace)

| Method | Description |
|---|---|
| `AddResourceFramework(Action<ResourceRegistry>)` | One-line setup: registers services, repos, controllers |

---

## Customizing a Resource Service

To add custom business logic for a specific resource, extend `ResourceService<T>`:

```csharp
using ResourceFramework.Server.Services;
using ResourceFramework.Server.DataAccess;
using ResourceFramework.Models;

public interface IMyCustomService : IResourceService<MyResource> { }

public class MyCustomService : ResourceService<MyResource>, IMyCustomService
{
    public MyCustomService(IResourceRepository<MyResource> repo, ILogger<MyCustomService> logger)
        : base(repo, logger) { }

    public override async Task<MyResource> Create(MyResource resource, string? key1 = null, ...)
    {
        // custom logic here
        return await base.Create(resource, key1, ...);
    }
}
```

Then register it using the overload:

```csharp
registry.AddResource<MyResource, IMyCustomService, MyCustomService>("MyResourcesTable");
```

---

## ResourceBase Properties

Every resource inherits these properties:

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | Unique identifier (auto-generated GUID) |
| `OwnerId` | `string?` | Optional owner association |
| `Key1` | `string?` | Generic lookup key 1 |
| `Key2` | `string?` | Generic lookup key 2 |
| `Key3` | `string?` | Generic lookup key 3 |
| `Version` | `int` | Auto-incremented on update |
| `CreatedAt` | `DateTime` | Set on creation |
| `UpdatedAt` | `DateTime?` | Set on every update |
| `IsDeleted` | `bool` | Soft-delete flag |

---

## Building & Publishing NuGet Packages

```bash
# Build all packages
dotnet build

# Pack with a specific version
dotnet pack ResourceFramework.Models -c Release -p:Version=1.0.0
dotnet pack ResourceFramework.Server -c Release -p:Version=1.0.0
dotnet pack UI.Component -c Release -p:Version=1.0.0

# Publish to NuGet.org
dotnet nuget push ResourceFramework.Models/bin/Release/ResourceFramework.Models.1.0.0.nupkg -k YOUR_API_KEY -s https://api.nuget.org/v3/index.json
dotnet nuget push ResourceFramework.Server/bin/Release/ResourceFramework.Server.1.0.0.nupkg -k YOUR_API_KEY -s https://api.nuget.org/v3/index.json
dotnet nuget push UI.Component/bin/Release/ResourceFramework.UI.1.0.0.nupkg -k YOUR_API_KEY -s https://api.nuget.org/v3/index.json
```

---

## Project Setup (Development)

```bash
git clone <repo-url>
cd DemonsAndDogs
dotnet restore
dotnet build
```

To run the API:
```bash
cd API
dotnet run
```

To run the Blazor WASM app:
```bash
cd DemonsAndDogs
dotnet run
```
