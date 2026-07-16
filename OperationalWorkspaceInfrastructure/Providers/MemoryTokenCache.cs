
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OperationalWorkspaceApplication.Abstractions;

namespace OperationalWorkspaceInfrastructure.Providers;

public class MemoryTokenCache : ITokenCache
{
    private readonly IMemoryCache _cache;
    public MemoryTokenCache(IMemoryCache cache) => _cache = cache;
    public bool TryGet(string key, out string? value) => _cache.TryGetValue(key, out value);
    public void Set(string key, string value, TimeSpan expiration) => _cache.Set(key, value, expiration);
    public void Remove(string key) => _cache.Remove(key);
}