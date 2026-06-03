namespace CalqFramework.Cmd.Shell.Durable;

/// <summary>
///     Persistence via GitHub Actions Cache internal REST API.
///     Auto-detected when ACTIONS_CACHE_URL and ACTIONS_RUNTIME_TOKEN are present.
///     Provides durability on ephemeral CI runners where filesystem is destroyed between attempts.
/// </summary>
internal sealed class GitHubActionsCacheDurabilityStore : IDurabilityStore {
    private const string VersionSalt = "calq-cmd-v1";
    private readonly List<string> _committedKeys = [];
    private readonly GitHubActionsEnvironment _env;
    private readonly HttpClient _httpClient;
    private readonly string _keyPrefix;
    private readonly ConcurrentDictionary<string, (int cacheId, MemoryStream stream)> _staged = new();
    private readonly string _version;

    public GitHubActionsCacheDurabilityStore(string workflowId, GitHubActionsEnvironment env) {
        _env = env;
        string scope = env.CommitSha?[..8] ?? "unknown";
        _keyPrefix = $"calq-cmd-{scope}-{workflowId}-";
        _httpClient = new HttpClient {
            BaseAddress = new Uri(env.CacheUrl)
        };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {env.RuntimeToken}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json;api-version=6.0-preview.1");

        byte[] versionHash = SHA256.HashData(Encoding.UTF8.GetBytes(VersionSalt));
        _version = Convert.ToHexString(versionHash)[..32]
            .ToLowerInvariant();
    }

    public Stream? Get(string key) {
        try {
            string fullKey = _keyPrefix + key;
            HttpResponseMessage response = _httpClient.GetAsync($"_apis/artifactcache/cache?keys={Uri.EscapeDataString(fullKey)}&version={_version}")
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            if (response.StatusCode == HttpStatusCode.NoContent) {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var json = JsonDocument.Parse(
                response.Content.ReadAsStreamAsync()
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult());

            if (!json.RootElement.TryGetProperty("archiveLocation", out JsonElement locationEl)) {
                return null;
            }

            HttpResponseMessage downloadResponse = _httpClient.GetAsync(locationEl.GetString()!)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            downloadResponse.EnsureSuccessStatusCode();

            byte[] data = downloadResponse.Content.ReadAsByteArrayAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            lock (_committedKeys) {
                _committedKeys.Add(fullKey);
            }

            return new MemoryStream(data, writable: false);
        } catch (HttpRequestException) {
            return null;
        } catch (TaskCanceledException) {
            return null;
        }
    }

    public Stream Create(string key) {
        string fullKey = _keyPrefix + key;
        var ms = new MemoryStream();
        try {
            string reserveBody = JsonSerializer.Serialize(
                new {
                    key = fullKey,
                    version = _version
                });
            HttpResponseMessage reserveResponse = _httpClient.PostAsync("_apis/artifactcache/caches", new StringContent(reserveBody, Encoding.UTF8, "application/json"))
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            if (reserveResponse.StatusCode == HttpStatusCode.Conflict) {
                _staged[fullKey] = (-1, ms);
                return ms;
            }

            reserveResponse.EnsureSuccessStatusCode();
            var reserveJson = JsonDocument.Parse(
                reserveResponse.Content.ReadAsStreamAsync()
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult());
            int cacheId = reserveJson.RootElement.GetProperty("cacheId")
                .GetInt32();
            _staged[fullKey] = (cacheId, ms);
        } catch (HttpRequestException) {
            _staged[fullKey] = (-1, ms);
        } catch (TaskCanceledException) {
            _staged[fullKey] = (-1, ms);
        }

        return ms;
    }

    public void Commit(string key) {
        string fullKey = _keyPrefix + key;
        if (!_staged.TryRemove(fullKey, out (int cacheId, MemoryStream stream) entry)) {
            return;
        }

        (int cacheId, MemoryStream? ms) = entry;

        if (cacheId == -1) {
            ms.Dispose();
            lock (_committedKeys) {
                _committedKeys.Add(fullKey);
            }

            return;
        }

        try {
            byte[] data = ms.ToArray();
            ms.Dispose();

            var content = new ByteArrayContent(data);
            content.Headers.Add("Content-Type", "application/octet-stream");
            content.Headers.Add("Content-Range", $"bytes 0-{data.Length - 1}/*");

            _httpClient.PatchAsync($"_apis/artifactcache/caches/{cacheId}", content)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            string finalizeBody = JsonSerializer.Serialize(
                new {
                    size = data.Length
                });
            _httpClient.PostAsync($"_apis/artifactcache/caches/{cacheId}", new StringContent(finalizeBody, Encoding.UTF8, "application/json"))
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            lock (_committedKeys) {
                _committedKeys.Add(fullKey);
            }
        } catch (HttpRequestException) {
        } catch (TaskCanceledException) {
        }
    }

    public void Discard(string key) {
        string fullKey = _keyPrefix + key;
        if (_staged.TryRemove(fullKey, out (int cacheId, MemoryStream stream) entry)) {
            entry.stream.Dispose();
        }
    }

    public void Clear() {
        if (string.IsNullOrEmpty(_env.GitHubToken) || string.IsNullOrEmpty(_env.Repository)) {
            return;
        }

        try {
            using var publicClient = new HttpClient {
                BaseAddress = new Uri("https://api.github.com/")
            };
            publicClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_env.GitHubToken}");
            publicClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            publicClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            publicClient.DefaultRequestHeaders.Add("User-Agent", "CalqFramework.Cmd");

            lock (_committedKeys) {
                foreach (string k in _committedKeys) {
                    try {
                        publicClient.DeleteAsync($"repos/{_env.Repository}/actions/caches?key={Uri.EscapeDataString(k)}")
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                    } catch {
                    }
                }

                _committedKeys.Clear();
            }
        } catch {
        }
    }

    public void Dispose() {
        foreach ((int _, MemoryStream? stream) in _staged.Values) {
            stream.Dispose();
        }

        _staged.Clear();
        _httpClient.Dispose();
    }


}
