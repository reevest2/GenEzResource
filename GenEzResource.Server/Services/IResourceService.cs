using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GenEzResource.Server.DataAccess;
using GenEzResource.Models;

namespace GenEzResource.Server.Services;

public interface IResourceService<TResource> where TResource : ResourceBase
{
    Task<List<TResource>> GetAll();
    Task<TResource> GetById(string resourceId);
    Task<TResource> GetByKeys(string? key1 = null, string? key2 = null, string? key3 = null);
    Task<List<TResource>> GetListByKeys(string? key1 = null, string? key2 = null, string? key3 = null);
    Task<int> GetCountByKeys(string? key1 = null, string? key2 = null, string? key3 = null, bool includeDeleted = false);
    Task<TResource> Create(TResource resource, string? key1 = null, string? key2 = null, string? key3 = null, string? ownerId = null);
    Task<TResource> Update(string resourceId, TResource resource);
    Task Delete(string resourceId, bool hardDelete = false);
}

public class ResourceService<TResource> : IResourceService<TResource> where TResource : ResourceBase
{
    protected readonly IResourceRepository<TResource> _resourceRepository;
    protected readonly ILogger<ResourceService<TResource>> _logger;

    public ResourceService(IResourceRepository<TResource> resourceRepository, ILogger<ResourceService<TResource>> logger)
    {
        _resourceRepository = resourceRepository;
        _logger = logger;
    }

    public virtual async Task<List<TResource>> GetAll()
    {
        return await _resourceRepository.GetAllAsync();
    }

    public virtual async Task<TResource> GetById(string resourceId)
    {
        return await _resourceRepository.GetByIdAsync(resourceId);
    }

    public virtual async Task<TResource> GetByKeys(string? key1 = null, string? key2 = null, string? key3 = null)
    {
        return await _resourceRepository.GetByKeysAsync(key1, key2, key3);
    }

    public virtual async Task<List<TResource>> GetListByKeys(string? key1 = null, string? key2 = null, string? key3 = null)
    {
        return await _resourceRepository.GetListByKeysAsync(key1, key2, key3);
    }

    public virtual async Task<int> GetCountByKeys(string? key1 = null, string? key2 = null, string? key3 = null, bool includeDeleted = false)
    {
        return await _resourceRepository.GetCountByKeysAsync(key1, key2, key3, includeDeleted);
    }

    public virtual async Task<TResource> Create(TResource resource, string? key1 = null, string? key2 = null, string? key3 = null, string? ownerId = null)
    {
        return await _resourceRepository.CreateResourceAsync(resource, key1, key2, key3, ownerId);
    }

    public virtual async Task<TResource> Update(string resourceId, TResource resource)
    {
        var storedResource = await _resourceRepository.GetByIdAsync(resourceId);
        return await _resourceRepository.UpdateResourceAsync(resourceId, resource);
    }

    public virtual async Task Delete(string resourceId, bool hardDelete = false)
    {
        var storedResource = await _resourceRepository.GetByIdAsync(resourceId);
        if (hardDelete)
        {
            await _resourceRepository.DeleteResourceAsync(resourceId);
        }
        else
        {
            await _resourceRepository.UpdateResourceAsync(resourceId, storedResource, true);
        }
    }
}
