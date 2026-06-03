using CalqFramework.Cmd.Shell.Durable;
using CalqFramework.Cmd.Shells;
using static CalqFramework.Cmd.Terminal;

namespace CalqFramework.Cmd.Tests;

public class DurabilityContextTest : IDisposable {
    private readonly FileSystemDurabilityStore _store;
    private readonly string _testDir;

    public DurabilityContextTest() {
        _testDir = Path.Combine(
            Path.GetTempPath(),
            "calq-cmd-ctx-test-" + Guid.NewGuid()
                .ToString("N")[..8]);
        _store = new FileSystemDurabilityStore(_testDir);
    }

    public void Dispose() {
        _store.Dispose();
        try {
            Directory.Delete(_testDir, true);
        } catch {
        }

        DurabilityContext.Current.Value = null;
    }

    [Fact]
    public void DurabilityContext_Default_IsNull() => Assert.Null(DurabilityContext.Current.Value);

    [Fact]
    public void DurabilityContext_WhenSet_IsAccessible() {
        DurableWorkflowContext ctx = new("wf-123", "path-abc", _store);
        DurabilityContext.Current.Value = ctx;

        Assert.Equal("wf-123", DurabilityContext.Current.Value!.WorkflowId);
        Assert.Equal("path-abc", DurabilityContext.Current.Value!.SequencePath);
        Assert.Same(_store, DurabilityContext.Current.Value!.Store);
    }

    [Fact]
    public async Task DurabilityContext_IsIsolatedAcrossTasks() {
        DurabilityContext.Current.Value = new DurableWorkflowContext("parent", "root", _store);

        string? childValue = null;
        await Task.Run(() => {
            childValue = DurabilityContext.Current.Value?.WorkflowId;
            DurabilityContext.Current.Value = new DurableWorkflowContext("child", "child-path", _store);
        });

        Assert.Equal("parent", childValue);
        Assert.Equal("parent", DurabilityContext.Current.Value?.WorkflowId);
    }

    [Fact]
    public void DurableWorkflowContext_RecordEquality() {
        DurableWorkflowContext ctx1 = new("wf", "path", _store);
        DurableWorkflowContext ctx2 = new("wf", "path", _store);
        DurableWorkflowContext ctx3 = new("wf", "other-path", _store);

        Assert.Equal(ctx1, ctx2);
        Assert.NotEqual(ctx1, ctx3);
    }

    [Fact]
    public void ShellSetter_WhenContextHasStore_WrapsWithDistributedStore() {
        // Arrange — set context with a store (simulating what LocalTerminalFilter does)
        DurabilityContext.Current.Value = new DurableWorkflowContext("server-wf", "seq", _store);

        // Act — set a raw shell (simulating what an attribute does)
        LocalTerminal.Shell = new CommandLine();

        // Assert — should be DurableShell using the context's store
        Assert.IsType<DurableShell>(LocalTerminal.Shell);
    }

    [Fact]
    public void ShellSetter_WhenNoContext_WrapsWithFilesystemStore() {
        // Arrange — no context (CLI scenario)
        DurabilityContext.Current.Value = null;

        // Act
        LocalTerminal.Shell = new CommandLine();

        // Assert — still DurableShell (filesystem default)
        Assert.IsType<DurableShell>(LocalTerminal.Shell);
    }

    [Fact]
    public void ShellSetter_WhenAlreadyDecorator_DoesNotDoubleWrap() {
        // Arrange
        CommandLine inner = new();
        DurableShell durable = new(inner, _store, "test-id");

        // Act
        LocalTerminal.Shell = durable;

        // Assert — passes through unchanged
        Assert.Same(durable, LocalTerminal.Shell);
    }
}
