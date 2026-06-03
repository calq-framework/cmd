namespace CalqFramework.Cmd.Shell.Durable;

/// <summary>
///     CLI persistence: {cacheDir}/{key}.bin
///     Lazy I/O: no filesystem operations until first Get or Create call.
///     Uses tmp+rename for atomic commits.
///     Thread-safe initialization via volatile flag.
/// </summary>
public sealed class FileSystemDurabilityStore(string cacheDir) : IDurabilityStore {
    private readonly string _cacheDir = cacheDir;
    private volatile bool _initialized;

    public Stream? Get(string key) {
        EnsureInitialized();
        string filePath = Path.Combine(_cacheDir, key + ".bin");
        return File.Exists(filePath) ? File.OpenRead(filePath) : null;
    }

    public Stream Create(string key) {
        EnsureInitialized();
        string tmpPath = Path.Combine(_cacheDir, key + ".bin.tmp");
        return new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan);
    }

    public void Commit(string key) {
        string tmpPath = Path.Combine(_cacheDir, key + ".bin.tmp");
        string finalPath = Path.Combine(_cacheDir, key + ".bin");
        File.Move(tmpPath, finalPath, overwrite: true);
    }

    public void Discard(string key) {
        string tmpPath = Path.Combine(_cacheDir, key + ".bin.tmp");
        try {
            File.Delete(tmpPath);
        } catch (IOException) {
        }
    }

    public void Clear() {
        try {
            File.WriteAllBytes(Path.Combine(_cacheDir, ".completed"), []);
            Directory.Delete(_cacheDir, true);
        } catch (IOException) {
        }
    }

    public void Dispose() { }

    private void EnsureInitialized() {
        if (_initialized) {
            return;
        }

        _initialized = true;

        string markerPath = Path.Combine(_cacheDir, ".completed");
        if (File.Exists(markerPath)) {
            try {
                Directory.Delete(_cacheDir, true);
            } catch (IOException) {
            }
        }

        Directory.CreateDirectory(_cacheDir);
    }
}
