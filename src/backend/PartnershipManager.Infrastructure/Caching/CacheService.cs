using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using PartnershipManager.Domain.Constants;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Caching;

/// <summary>
/// Implementação de cache usando Redis
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var cached = await _cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(cached))
            return null;

        return JsonSerializer.Deserialize<T>(cached, _jsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(SystemConstants.CacheExpirationMinutes)
        };

        var serialized = JsonSerializer.Serialize(value, _jsonOptions);
        await _cache.SetStringAsync(key, serialized, options);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        // Redis não suporta remoção por prefixo nativamente no IDistributedCache
        // Seria necessário usar StackExchange.Redis diretamente
        await Task.CompletedTask;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null) where T : class
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
            return cached;

        var value = await factory();
        if (value != null)
        {
            await SetAsync(key, value, expiration);
        }

        return value;
    }
}

/// <summary>
/// Implementação de cache em memória (fallback)
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public InMemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(SystemConstants.CacheExpirationMinutes)
        };

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix)
    {
        // MemoryCache não suporta remoção por prefixo
        return Task.CompletedTask;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? expiration = null) where T : class
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
            return cached;

        var value = await factory();
        if (value != null)
        {
            await SetAsync(key, value, expiration);
        }

        return value;
    }
}
