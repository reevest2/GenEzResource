# GenEzResource — Generic Resource Framework

A generic, convention-based resource framework built on ASP.NET Core 8, Entity Framework Core, and Blazor WebAssembly. Define a model, register it in one line, and get a full set of CRUD API endpoints **and** UI components automatically.

---

## NuGet Packages

The framework is distributed as two NuGet packages:

| Package | Description | Install In |
|---|---|---|
| **GenEzResource.Server** | All-in-one server package — includes `ResourceBase`, `Resource<T>`, EF Core repository, CRUD services, generic API controllers, `ResourceRegistry`, `ResourceDbContext`, and one-line DI setup | ASP.NET Core Web API projects |
| **GenEzResource.UI** | Blazor WASM components — Radzen DataGrid with CRUD, MediatR handlers, `ResourceUIRegistry` | Blazor WebAssembly projects |

Both packages share **GenEzResource.Models** (included automatically) which contains the `ResourceBase` and `Resource<T>` base classes.

### Package Dependency Graph

```
GenEzResource.Models          (standalone, no dependencies)
       │
       ├── GenEzResource.Server  (+ EF Core, ASP.NET Core MVC)
       │
       └── GenEzResource.UI     (+ Radzen.Blazor, MediatR)
```

---

## Quick Start — Server (API)

### 1. Install the package

```bash
dotnet add package GenEzResource.Server
```

Or reference the project directly:

```xml
<ProjectReference Include="..\GenEzResource.Server\GenEzResource.Server.csproj" />
```

### 2. Define a resource model

```csharp
using GenEzResource.Models;

namespace MyApp;

public class TodoItem : ResourceBase
{
    public string Title { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
}
```

### 3. Register it in `Program.cs` (one line!)

```csharp
using GenEzResource.Server.Extensions;
using GenEzResource.Server.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// One call registers: repository, service, EF table mapping, and API controller
builder.Services.AddGenEzResource(registry =>
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
dotnet add package GenEzResource.UI
```

### 2. Register resources in `Program.cs`

```csharp
using GenEzResource.UI.Extensions;

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddGenEzResourceUI(registry =>
{
    registry.AddResource<TodoItem>();
});
```

### 3. Add a resource page

```razor
@page "/resources/{ResourceName}"
@using GenEzResource.UI.Components

<GenEzResourceView ResourceName="@ResourceName" />
```

This single component handles the dynamic rendering of the appropriate resource grid based on the name from the URL. It is the easiest way to add resource management to your app.

### 4. Add navigation links

In your `NavMenu.razor`:

```razor
@using GenEzResource.UI.Components

<ResourceNavLinks />
```

This auto-generates sidebar links for every registered resource using their `DisplayName` and `RouteName`.

---

## Detailed UI Usage

### Using GenEzResourceView (Recommended)

`GenEzResourceView` is a "smart" component that looks up the resource type in the `ResourceUIRegistry` by its route name and renders a `ResourceGrid<T>` for it.

```razor
<GenEzResourceView ResourceName="TodoItems" />
```

### Using ResourceGrid<T> Directly

If you want more control, such as adding a resource grid to a custom page with other content, use `ResourceGrid<T>` directly:

```razor
@using GenEzResource.UI.Components
@using MyApp.Models

<ResourceGrid TResource="TodoItem" Title="My Custom Todo List" />
```

### Customizing Columns

By default, `ResourceGrid<T>` auto-generates columns for all properties of your model (plus `Id`). You can override this by providing a `Columns` render fragment:

```razor
<ResourceGrid TResource="TodoItem">
    <Columns>
        <RadzenDataGridColumn TItem="TodoItem" Property="Title" Title="Task Name" />
        <RadzenDataGridColumn TItem="TodoItem" Property="IsComplete" Title="Done">
            <Template Context="todo">
                <RadzenCheckBox @bind-Value="@todo.IsComplete" Disabled="true" />
            </Template>
        </RadzenDataGridColumn>
    </Columns>
</ResourceGrid>
```

---

## What's Inside GenEzResource

The framework provides three main packages:

### Models (`GenEzResource.Models` namespace)

| Class | Description |
|---|---|
| `ResourceBase` | Base class with `Id`, `OwnerId`, `Key1`–`Key3`, `Version`, `CreatedAt`, `UpdatedAt`, `IsDeleted` |
| `Resource<T>` | Wrapper that stores `T Data` as a JSONB column |

### Server (`GenEzResource.Server` namespace)

| Class / Interface | Description |
|---|---|
| `IResourceRepository<T>` | Repository interface with key-based queries, CRUD, and soft-delete |
| `ResourceRepository<T>` | EF Core implementation of the repository |
| `ResourceRegistry` | Fluent registry for adding resources (configures EF model + DI) |
| `ResourceDbContext` | Base DbContext that auto-applies resource model configurations |
| `IResourceService<T>` | Service interface for business logic layer |
| `ResourceService<T>` | Default implementation delegating to repository |
| `ResourceController<T>` | Generic API controller with 5 CRUD endpoints |
| `GenericResourceControllerFeatureProvider` | Auto-registers controllers for each resource type |
| `ResourceControllerModelConvention` | Names controllers after the resource type |

### UI Components (`GenEzResource.UI` namespace)

| Component | Description |
|---|---|
| `ResourceGrid<T>` | Radzen DataGrid with CRUD operations and MediatR integration |
| `ResourceEditDialog<T>` | Dialog for creating and editing resources |
| `ResourceNavLinks` | Automatically generated sidebar links for all registered resources |
| `GenEzResourceView` | A unified wrapper component that dynamically renders a `ResourceGrid<T>` by name |

---

## Customizing a Resource Service

To add custom business logic for a specific resource, extend `ResourceService<T>`:

```csharp
using GenEzResource.Server.Services;
using GenEzResource.Server.DataAccess;
using GenEzResource.Models;

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
dotnet pack GenEzResource.Models -c Release -p:Version=1.0.0
dotnet pack GenEzResource.Server -c Release -p:Version=1.0.0
dotnet pack GenEzResource.UI -c Release -p:Version=1.0.0

# Publish to NuGet.org
dotnet nuget push GenEzResource.Models/bin/Release/GenEzResource.Models.1.0.0.nupkg -k YOUR_API_KEY -s https://api.nuget.org/v3/index.json
dotnet nuget push GenEzResource.Server/bin/Release/GenEzResource.Server.1.0.0.nupkg -k YOUR_API_KEY -s https://api.nuget.org/v3/index.json
dotnet nuget push GenEzResource.UI/bin/Release/GenEzResource.UI.1.0.0.nupkg -k YOUR_API_KEY -s https://api.nuget.org/v3/index.json
```

---

## Project Setup (Development)

```bash
git clone <repo-url>
cd GenEzResource
dotnet restore
dotnet build
```

To run the API:
```bash
cd GenEzResource.Server
dotnet run
```

To run the Blazor WASM app:
```bash
cd GenEzResource.UI
dotnet run
```
