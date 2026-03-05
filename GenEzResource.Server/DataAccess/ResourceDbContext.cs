using Microsoft.EntityFrameworkCore;

namespace GenEzResource.Server.DataAccess;

/// <summary>
/// Base DbContext for the ResourceFramework. Automatically applies resource model
/// configurations from the <see cref="ResourceRegistry"/>.
/// Consumers can use this directly or extend it to add Identity, additional entities, etc.
/// </summary>
public class ResourceDbContext : DbContext
{
    private readonly ResourceRegistry _resourceRegistry;

    public ResourceDbContext(DbContextOptions options, ResourceRegistry resourceRegistry)
        : base(options)
    {
        _resourceRegistry = resourceRegistry;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _resourceRegistry.ApplyModelConfigurations(modelBuilder);
    }
}
