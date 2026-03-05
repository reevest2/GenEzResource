using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using GenEzResource.Models;

namespace GenEzResource.Server.DataAccess;

public interface IResourceRepository<T> where T : ResourceBase
{
    DbSet<Resource<T>> GetDbSet();
    IQueryable<Resource<T>> GetQuery();
    Task<List<T>> GetAllAsync(params Expression<Func<Resource<T>, bool>>[] filters);
    Task<T> GetByIdAsync(string id);
    Task<T> GetByKeysAsync(string? key1 = null, string? key2 = null, string? key3 = null);

    Task<T> FirstOrDefaultAsync(string? key1 = null, string? key2 = null, string? key3 = null,
        params Expression<Func<Resource<T>, bool>>[] filters);

    Task<List<T>> GetListByKeysAsync(string? key1 = null, string? key2 = null, string? key3 = null);

    Task<int> GetCountByKeysAsync(string? key1 = null, string? key2 = null, string? key3 = null,
        bool includeDeleted = false);

    Task<T> CreateResourceAsync(T data, string? key1 = null, string? key2 = null, string? key3 = null,
        string? ownerId = null);

    Task<T> UpdateResourceAsync(string id, T data, bool isDeleted = false);
    Task DeleteResourceAsync(string id);
}