using System.Collections.Generic;
using GenEzResource.Models;
using MediatR;

namespace GenEzResource.UI.MediatR;

public record GetAllResourcesQuery<T>() : IRequest<List<T>> where T : ResourceBase;

public record GetResourceByIdQuery<T>(string Id) : IRequest<T?> where T : ResourceBase;

public record GetResourcesByFilterQuery<T>(
    string? Key1 = null,
    string? Key2 = null,
    string? Key3 = null
) : IRequest<List<T>> where T : ResourceBase;

public record UpsertResourceCommand<T>(
    T Data,
    string? Id = null,
    string? Key1 = null,
    string? Key2 = null,
    string? Key3 = null,
    string? OwnerId = null
) : IRequest<T> where T : ResourceBase;

public record SoftDeleteResourceCommand<T>(string Id) : IRequest<bool> where T : ResourceBase;
