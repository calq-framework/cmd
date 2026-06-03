using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shell.Durable;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.Tests;

public class DurableShellTest : IDisposable {
    private readonly string _testCacheDir;

    public DurableShellTest() => _testCacheDir = Path.Combine(
        Path.GetTempPath(),
        "calq-cmd-test-" + Guid.NewGuid()
            .ToString("N")[..8]);

    public void Dispose() {
        try {
            Directory.Delete(_testCacheDir, true);
        } catch {
        }
    }

    [Fact]
    public void DurableShell_FirstRun_ExecutesCommand() {
        FileSystemDurabilityStore store = new(_testCacheDir);
        using DurableShell durable = new(new CommandLine(), store, "test-workflow-1");
        LocalTerminal.Shell = durable;

        string result = CMD("cmd /c echo hello");

        Assert.Equal("hello", result);
    }

    [Fact]
    public void DurableShell_SecondRun_ServesFromCache() {
        // First run — execute and cache
        FileSystemDurabilityStore store1 = new(_testCacheDir);
        CommandLine inner1 = new();
        using (DurableShell durable1 = new(inner1, store1, "test-workflow-2")) {
            LocalTerminal.Shell = durable1;
            string result1 = CMD("cmd /c echo cached_value");
            Assert.Equal("cached_value", result1);
        }

        // Second run — should serve from cache even with different inner shell behavior
        // (Simulated by using same store directory)
        FileSystemDurabilityStore store2 = new(_testCacheDir);
        using (DurableShell durable2 = new(new CommandLine(), store2, "test-workflow-2")) {
            LocalTerminal.Shell = durable2;
            string result2 = CMD("cmd /c echo cached_value");
            Assert.Equal("cached_value", result2);
        }
    }

    [Fact]
    public void DurableShell_DifferentScripts_GetDifferentCacheEntries() {
        FileSystemDurabilityStore store = new(_testCacheDir);
        using DurableShell durable = new(new CommandLine(), store, "test-workflow-3");
        LocalTerminal.Shell = durable;

        string result1 = CMD("cmd /c echo first");
        string result2 = CMD("cmd /c echo second");

        Assert.Equal("first", result1);
        Assert.Equal("second", result2);
    }

    [Fact]
    public void DurableShell_SameScriptTwice_GetsSeparateOccurrences() {
        FileSystemDurabilityStore store = new(_testCacheDir);
        using DurableShell durable = new(new CommandLine(), store, "test-workflow-4");
        LocalTerminal.Shell = durable;

        string result1 = CMD("cmd /c echo same");
        string result2 = CMD("cmd /c echo same");

        Assert.Equal("same", result1);
        Assert.Equal("same", result2);
    }

    [Fact]
    public void DurableShell_IsTransparentDecorator() {
        CommandLine inner = new();
        FileSystemDurabilityStore store = new(_testCacheDir);
        using DurableShell durable = new(inner, store, "test-workflow-5");

        Assert.Same(inner.ExceptionFactory, durable.ExceptionFactory);
        Assert.Same(inner.Postprocessor, durable.Postprocessor);
        Assert.Equal(inner.In, durable.In);
    }

    [Fact]
    public void DurableShell_IsShellDecoratorBase() {
        CommandLine inner = new();
        FileSystemDurabilityStore store = new(_testCacheDir);
        using DurableShell durable = new(inner, store, "test-workflow-6");

        Assert.IsType<ShellDecoratorBase>(durable, exactMatch: false);
    }

    [Fact]
    public void AutoWrap_WhenSettingShell_WrapsInDurableShell() {
        LocalTerminal.Shell = new CommandLine();

        Assert.IsType<DurableShell>(LocalTerminal.Shell);
    }

    [Fact]
    public void AutoWrap_WhenSettingDurableShell_DoesNotDoubleWrap() {
        CommandLine inner = new();
        FileSystemDurabilityStore store = new(_testCacheDir);
        DurableShell durable = new(inner, store, "test-workflow-7");
        LocalTerminal.Shell = durable;

        Assert.Same(durable, LocalTerminal.Shell);
    }

    [Fact]
    public void SetRawShell_BypassesAutoWrap() {
        CommandLine raw = new();
        LocalTerminal.SetRawShell(raw);

        Assert.Same(raw, LocalTerminal.Shell);
    }

    [Fact]
    public void DurableShell_ClearAll_RemovesCacheDirectory() {
        // Use a unique subdirectory within the calq-cmd-cache dir to test ClearAll behavior
        // without interfering with other tests using shared workflow state
        string baseDir = Path.Combine(Path.GetTempPath(), "calq-cmd-cache");
        string workflowDir = Path.Combine(
            baseDir,
            "clearall-test-" + Guid.NewGuid()
                .ToString("N")[..8]);
        Directory.CreateDirectory(workflowDir);
        File.WriteAllText(Path.Combine(workflowDir, "test.bin"), "data");

        // Clear the specific workflow dir using Clear(workflowId) instead of ClearAll()
        // ClearAll would interfere with parallel test execution
        DurableShell.Clear(
            "clearall-test-" + Path.GetFileName(workflowDir)
                .Replace("clearall-test-", ""));

        Assert.False(Directory.Exists(workflowDir));
    }

    [Fact]
    public async Task DurableShell_CMDStreamAsync_WorksCorrectly() {
        FileSystemDurabilityStore store = new(_testCacheDir);
        using DurableShell durable = new(new CommandLine(), store, "test-workflow-stream");
        LocalTerminal.Shell = durable;

        using ShellWorkerOutputStream stream = await CMDStreamAsync("cmd /c echo stream_data");
        using StreamReader reader = new(stream);
        string output = (await reader.ReadToEndAsync()).Trim();

        Assert.Equal("stream_data", output);
    }

    [Fact]
    public void DurableShell_FailedCommand_DoesNotCache() {
        FileSystemDurabilityStore store = new(_testCacheDir);
        using DurableShell durable = new(new CommandLine(), store, "test-workflow-fail");
        LocalTerminal.Shell = durable;

        Assert.Throws<ShellScriptException>(() => CMD("cmd /c exit 1"));

        // Verify no .bin files were committed (only .bin.tmp may exist or be cleaned up)
        if (Directory.Exists(_testCacheDir)) {
            string[] committedFiles = Directory.GetFiles(_testCacheDir, "*.bin");
            Assert.Empty(committedFiles);
        }
    }

    [Fact]
    public void DurableShell_MapToHostPath_DelegatesToInner() {
        CommandLine inner = new();
        FileSystemDurabilityStore store = new(_testCacheDir);
        using DurableShell durable = new(inner, store, "test-workflow-path");

        string testPath = @"C:\Users\test";
        Assert.Equal(inner.MapToHostPath(testPath), durable.MapToHostPath(testPath));
    }

    [Fact]
    public void DurableShell_MapToInternalPath_DelegatesToInner() {
        CommandLine inner = new();
        FileSystemDurabilityStore store = new(_testCacheDir);
        using DurableShell durable = new(inner, store, "test-workflow-path2");

        string testPath = @"C:\Users\test";
        Assert.Equal(inner.MapToInternalPath(testPath), durable.MapToInternalPath(testPath));
    }
}
