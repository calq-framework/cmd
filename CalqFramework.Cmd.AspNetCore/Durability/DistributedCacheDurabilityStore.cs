using CalqFramework.Cmd.Shell.Durable;

namespace CalqFramework.Cmd.AspNetCore.Durability;

/// <summary>
///     ASP.NET Core persistence via IDistributedCache. 30-day TTL as safety net for crashes.
///     Lives in CalqFramework.Cmd.AspNetCore to keep core package dependency-free.
/// </summary>
internal sealed class DistributedCacheDurabilityStore : IDurabilityStore {
    private readonly IDistributedCache _cache;
    private readonly List<string> _committedKeys = [];
    private readonly DistributedCacheEntryOptions _entryOptions;
    private readonly string _keyPrefix;
    private readonly ConcurrentDictionary<string, MemoryStream> _staged = new();

    public DistributedCacheDurabilityStore(IDistributedCache cache, string keyPrefix, TimeSpan? ttl = null) {
        _cache = cache;
        _keyPrefix = keyPrefix;
        _entryOptions = new DistributedCacheEntryOptions {
            AbsoluteExpirationRelativeToNow = ttl ?? TimeSpan.FromDays(30)
        };
    }

    public Stream? Get(string key) {
        string fullKey = _keyPrefix + key;
        byte[]? data = _cache.Get(fullKey);
        if (data != null) {
            lock (_committedKeys) {
                _committedKeys.Add(fullKey);
            }

            return new MemoryStream(data, writable: false);
        }

        return null;
    }

    public Stream Create(string key) {
        string fullKey = _keyPrefix + key;
        var ms = new MemoryStream();
        _staged[fullKey] = ms;
        return ms;
    }

    public void Commit(string key) {
        string fullKey = _keyPrefix + key;
        if (_staged.TryRemove(fullKey, out MemoryStream? ms)) {
            _cache.Set(fullKey, ms.ToArray(), _entryOptions);
            ms.Dispose();
            lock (_committedKeys) {
                _committedKeys.Add(fullKey);
            }
        }
    }

    public void Discard(string key) {
        string fullKey = _keyPrefix + key;
        if (_staged.TryRemove(fullKey, out MemoryStream? ms)) {
            ms.Dispose();
        }
    }

    public void Clear() {
        lock (_committedKeys) {
            foreach (string k in _committedKeys) {
                _cache.Remove(k);
            }

            _committedKeys.Clear();
        }
    }

    public void Dispose() { }
}
