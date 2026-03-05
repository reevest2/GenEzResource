using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ResourceFramework.Models;

namespace ResourceFramework.Server.DataAccess;

public class ResourceRepository<TResource> : IResourceRepository<TResource> where TResource : ResourceBase
{
    protected readonly DbContext _context;

    public ResourceRepository(DbContext context)
    {
        _context = context;
    }

    public DbSet<Resource<TResource>> GetDbSet() => _context.Set<Resource<TResource>>();
    public IQueryable<Resource<TResource>> GetQuery() => _context.Set<Resource<TResource>>().AsQueryable();

    private IQueryable<Resource<TResource>> ApplyKeyFilters(IQueryable<Resource<TResource>> query, string? key1, string? key2, string? key3)
    {
        if (key1 != null)
            query = query.Where(r => r.Key1 == key1);
        if (key2 != null)
            query = query.Where(r => r.Key2 == key2);
        if (key3 != null)
            query = query.Where(r => r.Key3 == key3);
        return query;
    }

    public virtual async Task<List<TResource>> GetAllAsync(params Expression<Func<Resource<TResource>, bool>>[] filters)
    {
        var query = _context.Set<Resource<TResource>>().Where(r => !r.IsDeleted);
        if (filters is { Length: > 0 })
        {
            query = filters.Aggregate(query, (current, filter) => current.Where(filter));
        }
        return await query.Select(r => r.Data).ToListAsync();
    }

    public virtual async Task<TResource> GetByIdAsync(string id)
    {
        var resource = await _context.Set<Resource<TResource>>().FindAsync(id);
        if (resource == null || resource.IsDeleted)
        {
            throw new NotImplementedException(id);
        }
        return resource.Data;
    }

    public virtual async Task<TResource> GetByKeysAsync(string? key1 = null, string? key2 = null, string? key3 = null)
    {
        var query = _context.Set<Resource<TResource>>().AsQueryable();
        query = ApplyKeyFilters(query, key1, key2, key3);
        var resource = await query.FirstOrDefaultAsync(r => !r.IsDeleted);
        if (resource == null)
        {
            throw new NotImplementedException();
        }
        return resource.Data;
    }
    
    public virtual async Task<TResource> FirstOrDefaultAsync(string? key1 = null, string? key2 = null, string? key3 = null, params Expression<Func<Resource<TResource>, bool>>[] filters)
    {
        var query = _context.Set<Resource<TResource>>().AsQueryable();
        query = ApplyKeyFilters(query, key1, key2, key3);
        if (filters is { Length: > 0 })
        {
            query = filters.Aggregate(query, (current, filter) => current.Where(filter));
        }
        var resource = await query.FirstOrDefaultAsync(r => !r.IsDeleted);
        if (resource == null)
        {
            return default;
        }
        return resource.Data;
    }
    
    public virtual async Task<List<TResource>> GetListByKeysAsync(string? key1 = null, string? key2 = null, string? key3 = null)
    {
        var query = _context.Set<Resource<TResource>>().Where(r => !r.IsDeleted);
        query = ApplyKeyFilters(query, key1, key2, key3);
        return await query
            .OrderByDescending(r => r.UpdatedAt)
            .Select(r => r.Data).ToListAsync();
    }
    
    public async Task<int> GetCountByKeysAsync(string? key1 = null, string? key2 = null, string? key3 = null, bool includeDeleted = false)
    {
        var query = _context.Set<Resource<TResource>>().AsQueryable();
        query = ApplyKeyFilters(query, key1, key2, key3);
        
        if (!includeDeleted)
            query = query.Where(r => !r.IsDeleted);
        
        return await query.CountAsync();
    }

    public virtual async Task<TResource> CreateResourceAsync(TResource data, string? key1 = null, string? key2 = null, string? key3 = null, string? ownerId = null)
    {
        var timestamp = DateTime.UtcNow;

        var resource = new Resource<TResource>
        {
            Id = Guid.NewGuid().ToString(),
            Key1 = key1,
            Key2 = key2,
            Key3 = key3,
            OwnerId = ownerId,
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
            Version = 1,
            IsDeleted = false,
            Data = data
        };

        await _context.Set<Resource<TResource>>().AddAsync(resource);
        await _context.SaveChangesAsync();
        return resource.Data;
    }

    public virtual async Task<TResource> UpdateResourceAsync(string id, TResource data, bool isDeleted = false)
    {
        var storedResource = await _context.Set<Resource<TResource>>().FindAsync(id);
        if (storedResource == null || storedResource.IsDeleted)
            throw new NotImplementedException();

        storedResource.Version += 1;
        storedResource.UpdatedAt = DateTime.UtcNow;
        storedResource.Data = data;
        storedResource.IsDeleted = isDeleted;

        _context.Set<Resource<TResource>>().Update(storedResource);
        await _context.SaveChangesAsync();
        return storedResource.Data;
    }
    
    public virtual async Task DeleteResourceAsync(string id)
    {
        var storedResource = await _context.Set<Resource<TResource>>().FindAsync(id);
        if (storedResource == null)
            return;

        _context.Set<Resource<TResource>>().Remove(storedResource);
        await _context.SaveChangesAsync();
    }
}
