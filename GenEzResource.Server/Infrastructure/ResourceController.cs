using Microsoft.AspNetCore.Mvc;
using GenEzResource.Models;
using GenEzResource.Server.Services;

namespace GenEzResource.Server.Infrastructure;

public record UpsertResourceRequest<T>(
    T Data,
    string? Id = null,
    string? Key1 = null,
    string? Key2 = null,
    string? Key3 = null,
    string? OwnerId = null
);

[ApiController]
[Route("api/[controller]")]
public class ResourceController<T> : ControllerBase where T : ResourceBase
{
    private readonly IResourceService<T> _service;

    public ResourceController(IResourceService<T> service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<T>> GetById(string id)
    {
        var result = await _service.GetById(id);
        return Ok(result);
    }

    [HttpGet("filter")]
    public async Task<ActionResult<List<T>>> GetListByFilter(
        [FromQuery] string? key1 = null,
        [FromQuery] string? key2 = null,
        [FromQuery] string? key3 = null)
    {
        var result = await _service.GetListByKeys(key1, key2, key3);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<T>>> GetAll()
    {
        var result = await _service.GetAll();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<T>> Upsert([FromBody] UpsertResourceRequest<T> request)
    {
        T result;
        if (!string.IsNullOrEmpty(request.Id))
        {
            result = await _service.Update(request.Id, request.Data);
        }
        else
        {
            result = await _service.Create(request.Data, request.Key1, request.Key2, request.Key3, request.OwnerId);
        }
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> SoftDelete(string id)
    {
        await _service.Delete(id, hardDelete: false);
        return NoContent();
    }
}
