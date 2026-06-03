using CalqFramework.Cmd.Shell.Durable;

namespace CalqFramework.Cmd.Tests;

public class FileSystemDurabilityStoreTest : IDisposable {
    private readonly string _testDir;

    public FileSystemDurabilityStoreTest() => _testDir = Path.Combine(
        Path.GetTempPath(),
        "calq-cmd-store-test-" + Guid.NewGuid()
            .ToString("N")[..8]);

    public void Dispose() {
        try {
            Directory.Delete(_testDir, true);
        } catch {
        }
    }

    [Fact]
    public void Get_WhenKeyNotExists_ReturnsNull() {
        using FileSystemDurabilityStore store = new(_testDir);

        Stream? result = store.Get("nonexistent-key");

        Assert.Null(result);
    }

    [Fact]
    public void CreateAndCommit_ThenGet_ReturnsData() {
        using FileSystemDurabilityStore store = new(_testDir);
        byte[] data = "hello world"u8.ToArray();

        using (Stream writeStream = store.Create("test-key")) {
            writeStream.Write(data);
        }

        store.Commit("test-key");

        using Stream? readStream = store.Get("test-key");
        Assert.NotNull(readStream);
        byte[] buffer = new byte[data.Length];
        int read = readStream!.Read(buffer);
        Assert.Equal(data.Length, read);
        Assert.Equal(data, buffer);
    }

    [Fact]
    public void Create_WithoutCommit_GetReturnsNull() {
        using FileSystemDurabilityStore store = new(_testDir);
        byte[] data = "uncommitted"u8.ToArray();

        using (Stream writeStream = store.Create("uncommitted-key")) {
            writeStream.Write(data);
        }

        // Don't commit — should not be visible via Get
        Stream? result = store.Get("uncommitted-key");
        Assert.Null(result);
    }

    [Fact]
    public void Discard_RemovesStagedEntry() {
        using FileSystemDurabilityStore store = new(_testDir);

        using (Stream writeStream = store.Create("discard-key")) {
            writeStream.Write("data"u8.ToArray());
        }

        store.Discard("discard-key");

        // Verify .tmp file is removed
        string tmpPath = Path.Combine(_testDir, "discard-key.bin.tmp");
        Assert.False(File.Exists(tmpPath));
    }

    [Fact]
    public void Clear_RemovesCacheDirectory() {
        using FileSystemDurabilityStore store = new(_testDir);

        // Create and commit an entry to ensure directory exists
        using (Stream writeStream = store.Create("clear-key")) {
            writeStream.Write("data"u8.ToArray());
        }

        store.Commit("clear-key");
        Assert.True(Directory.Exists(_testDir));

        store.Clear();

        Assert.False(Directory.Exists(_testDir));
    }

    [Fact]
    public void LazyInit_DoesNotCreateDirectoryOnConstruction() {
        string testDir = Path.Combine(
            Path.GetTempPath(),
            "calq-cmd-lazy-" + Guid.NewGuid()
                .ToString("N")[..8]);
        FileSystemDurabilityStore store = new(testDir);

        Assert.False(Directory.Exists(testDir));

        store.Dispose();
    }

    [Fact]
    public void LazyInit_CreatesDirectoryOnFirstGet() {
        using FileSystemDurabilityStore store = new(_testDir);

        store.Get("any-key");

        Assert.True(Directory.Exists(_testDir));
    }

    [Fact]
    public void CompletedMarker_TriggersCleanupOnNextInit() {
        // First: create directory and write .completed marker
        Directory.CreateDirectory(_testDir);
        File.WriteAllText(Path.Combine(_testDir, "old-entry.bin"), "stale data");
        File.WriteAllBytes(Path.Combine(_testDir, ".completed"), []);

        // Second: new store should detect marker and clean up
        using FileSystemDurabilityStore store = new(_testDir);
        store.Get("trigger-init"); // triggers lazy init

        // The old entry should be gone
        Assert.False(File.Exists(Path.Combine(_testDir, "old-entry.bin")));
        // Directory should still exist (recreated)
        Assert.True(Directory.Exists(_testDir));
    }

    [Fact]
    public void Commit_OverwritesExistingEntry() {
        using FileSystemDurabilityStore store = new(_testDir);

        // First entry
        using (Stream s1 = store.Create("overwrite-key")) {
            s1.Write("first"u8.ToArray());
        }

        store.Commit("overwrite-key");

        // Second entry overwrites
        using (Stream s2 = store.Create("overwrite-key")) {
            s2.Write("second"u8.ToArray());
        }

        store.Commit("overwrite-key");

        // Should read second value
        using Stream? result = store.Get("overwrite-key");
        Assert.NotNull(result);
        using StreamReader reader = new(result!);
        Assert.Equal("second", reader.ReadToEnd());
    }

    [Fact]
    public void MultipleKeys_IndependentEntries() {
        using FileSystemDurabilityStore store = new(_testDir);

        using (Stream s1 = store.Create("key-a")) {
            s1.Write("alpha"u8.ToArray());
        }

        using (Stream s2 = store.Create("key-b")) {
            s2.Write("beta"u8.ToArray());
        }

        store.Commit("key-a");
        store.Commit("key-b");

        using Stream? ra = store.Get("key-a");
        using Stream? rb = store.Get("key-b");

        Assert.NotNull(ra);
        Assert.NotNull(rb);

        using StreamReader readerA = new(ra!);
        using StreamReader readerB = new(rb!);

        Assert.Equal("alpha", readerA.ReadToEnd());
        Assert.Equal("beta", readerB.ReadToEnd());
    }
}
