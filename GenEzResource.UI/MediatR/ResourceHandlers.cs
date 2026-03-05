using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GenEzResource.Models;
using GenEzResource.UI.Services;
using MediatR;

namespace GenEzResource.UI.MediatR;

public class GetAllResourcesHandler<T> : IRequestHandler<GetAllResourcesQuery<T>, List<T>> where T : ResourceBase
{
    private readonly HttpClient _http;
    private readonly ResourceUIRegistry _registry;

    public GetAllResourcesHandler(HttpClient http, ResourceUIRegistry registry)
    {
        _http = http;
        _registry = registry;
    }

    public async Task<List<T>> Handle(GetAllResourcesQuery<T> request, CancellationToken cancellationToken)
    {
        var route = _registry.GetRouteName<T>();
        var result = await _http.GetFromJsonAsync<List<T>>($"api/{route}", cancellationToken);
        return result ?? new List<T>();
    }
}

public class GetResourceByIdHandler<T> : IRequestHandler<GetResourceByIdQuery<T>, T?> where T : ResourceBase
{
    private readonly HttpClient _http;
    private readonly ResourceUIRegistry _registry;

    public GetResourceByIdHandler(HttpClient http, ResourceUIRegistry registry)
    {
        _http = http;
        _registry = registry;
    }

    public async Task<T?> Handle(GetResourceByIdQuery<T> request, CancellationToken cancellationToken)
    {
        var route = _registry.GetRouteName<T>();
        return await _http.GetFromJsonAsync<T>($"api/{route}/{request.Id}", cancellationToken);
    }
}

public class GetResourcesByFilterHandler<T> : IRequestHandler<GetResourcesByFilterQuery<T>, List<T>> where T : ResourceBase
{
    private readonly HttpClient _http;
    private readonly ResourceUIRegistry _registry;

    public GetResourcesByFilterHandler(HttpClient http, ResourceUIRegistry registry)
    {
        _http = http;
        _registry = registry;
    }

    public async Task<List<T>> Handle(GetResourcesByFilterQuery<T> request, CancellationToken cancellationToken)
    {
        var route = _registry.GetRouteName<T>();
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(request.Key1)) queryParams.Add($"key1={Uri.EscapeDataString(request.Key1)}");
        if (!string.IsNullOrEmpty(request.Key2)) queryParams.Add($"key2={Uri.EscapeDataString(request.Key2)}");
        if (!string.IsNullOrEmpty(request.Key3)) queryParams.Add($"key3={Uri.EscapeDataString(request.Key3)}");

        var url = $"api/{route}/filter";
        if (queryParams.Count > 0)
            url += "?" + string.Join("&", queryParams);

        var result = await _http.GetFromJsonAsync<List<T>>(url, cancellationToken);
        return result ?? new List<T>();
    }
}

public class UpsertResourceHandler<T> : IRequestHandler<UpsertResourceCommand<T>, T> where T : ResourceBase
{
    private readonly HttpClient _http;
    private readonly ResourceUIRegistry _registry;

    public UpsertResourceHandler(HttpClient http, ResourceUIRegistry registry)
    {
        _http = http;
        _registry = registry;
    }

    public async Task<T> Handle(UpsertResourceCommand<T> request, CancellationToken cancellationToken)
    {
        var route = _registry.GetRouteName<T>();
        var payload = new
        {
            request.Data,
            request.Id,
            request.Key1,
            request.Key2,
            request.Key3,
            request.OwnerId
        };

        var response = await _http.PostAsJsonAsync($"api/{route}", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
        return result!;
    }
}

public class SoftDeleteResourceHandler<T> : IRequestHandler<SoftDeleteResourceCommand<T>, bool> where T : ResourceBase
{
    private readonly HttpClient _http;
    private readonly ResourceUIRegistry _registry;

    public SoftDeleteResourceHandler(HttpClient http, ResourceUIRegistry registry)
    {
        _http = http;
        _registry = registry;
    }

    public async Task<bool> Handle(SoftDeleteResourceCommand<T> request, CancellationToken cancellationToken)
    {
        var route = _registry.GetRouteName<T>();
        var response = await _http.DeleteAsync($"api/{route}/{request.Id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
