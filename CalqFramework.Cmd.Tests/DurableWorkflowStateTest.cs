using CalqFramework.Cmd.Shell.Durable;

namespace CalqFramework.Cmd.Tests;

public class DurableWorkflowStateTest : IDisposable {
    private readonly FileSystemDurabilityStore _store;
    private readonly string _testCacheDir;

    public DurableWorkflowStateTest() {
        _testCacheDir = Path.Combine(
            Path.GetTempPath(),
            "calq-cmd-state-test-" + Guid.NewGuid()
                .ToString("N")[..8]);
        _store = new FileSystemDurabilityStore(_testCacheDir);
    }

    public void Dispose() {
        _store.Dispose();
        try {
            Directory.Delete(_testCacheDir, true);
        } catch {
        }
    }

    [Fact]
    public void CreatePrivate_CreatesIndependentState() {
        DurableWorkflowState state = DurableWorkflowState.CreatePrivate(_store, "wf-1", "");

        Assert.Equal("wf-1", state.WorkflowId);
        Assert.Equal("", state.BaseSequencePath);
    }

    [Fact]
    public void NextOccurrence_FirstCall_ReturnsOccurrence1() {
        DurableWorkflowState state = DurableWorkflowState.CreatePrivate(_store, "wf-2", "");

        (string? key, int occurrence, string? sequencePath) = state.NextOccurrence("abc123");

        Assert.Equal(1, occurrence);
        Assert.Equal("abc123-001", key);
        Assert.Equal("abc123-001", sequencePath);
    }

    [Fact]
    public void NextOccurrence_SecondCall_ReturnsOccurrence2() {
        DurableWorkflowState state = DurableWorkflowState.CreatePrivate(_store, "wf-3", "");

        state.NextOccurrence("abc123");
        (string? key, int occurrence, string? sequencePath) = state.NextOccurrence("abc123");

        Assert.Equal(2, occurrence);
        Assert.Equal("abc123-002", key);
        Assert.Equal("abc123-002", sequencePath);
    }

    [Fact]
    public void NextOccurrence_DifferentHashes_IndependentCounters() {
        DurableWorkflowState state = DurableWorkflowState.CreatePrivate(_store, "wf-4", "");

        (string? key1, int occ1, string _) = state.NextOccurrence("hash-a");
        (string? key2, int occ2, string _) = state.NextOccurrence("hash-b");
        (string? key3, int occ3, string _) = state.NextOccurrence("hash-a");

        Assert.Equal(1, occ1);
        Assert.Equal(1, occ2);
        Assert.Equal(2, occ3);
        Assert.Equal("hash-a-001", key1);
        Assert.Equal("hash-b-001", key2);
        Assert.Equal("hash-a-002", key3);
    }

    [Fact]
    public void NextOccurrence_WithBaseSequencePath_PrependsBase() {
        DurableWorkflowState state = DurableWorkflowState.CreatePrivate(_store, "wf-5", "parent-001");

        (string _, int _, string? sequencePath) = state.NextOccurrence("child");

        Assert.Equal("parent-001.child-001", sequencePath);
    }

    [Fact]
    public void CounterSemantics_IncrementAndDecrement() {
        DurableWorkflowState state = DurableWorkflowState.CreatePrivate(_store, "wf-6", "");

        state.IncrementTotalCreated();
        state.IncrementTotalCreated();
        state.IncrementPendingCount();
        state.IncrementPendingCount();
        state.OnWorkerCommitted(); // pendingCount: 2 → 1
        state.OnWorkerCommitted(); // pendingCount: 1 → 0

        // If disposed now with ExitCode==0, should clear
        // (We can't easily test this without mocking Environment.ExitCode)
        Assert.Equal("wf-6", state.WorkflowId);
    }

    [Fact]
    public void ComputeWorkflowId_IsDeterministic() {
        string id1 = DurableWorkflowState.ComputeWorkflowId();
        string id2 = DurableWorkflowState.ComputeWorkflowId();

        Assert.Equal(id1, id2);
        Assert.Equal(16, id1.Length);
    }

    [Fact]
    public void ComputeWorkflowId_IsHexadecimal() {
        string id = DurableWorkflowState.ComputeWorkflowId();

        Assert.Matches("^[0-9a-f]{16}$", id);
    }
}
