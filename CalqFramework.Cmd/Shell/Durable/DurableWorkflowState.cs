namespace CalqFramework.Cmd.Shell.Durable;

/// <summary>
///     Shared state for all DurableShell instances within the same workflow.
///     Keyed by workflow ID — multiple DurableShell instances (from auto-wrap)
///     share one state object to ensure correct completion detection.
///     Owns ProcessExit registration and cleanup lifecycle.
///     Auto-detects CI environment for store selection.
/// </summary>
internal class DurableWorkflowState : IDisposable {
    private static readonly ConcurrentDictionary<string, DurableWorkflowState> s_states = new();
    private readonly ConcurrentDictionary<string, int> _hashOccurrences = new();

    private int _disposed;
    private int _pendingCount;
    private int _totalCreated;

    private DurableWorkflowState(IDurabilityStore store, string workflowId, string baseSequencePath) {
        Store = store;
        WorkflowId = workflowId;
        BaseSequencePath = baseSequencePath;
    }

    public string WorkflowId { get; }
    public string BaseSequencePath { get; }
    public IDurabilityStore Store { get; }

    /// <summary>
    ///     Gets or creates shared state for an explicit workflow ID.
    ///     Auto-detects store: GitHub Actions Cache on ephemeral CI runners, filesystem otherwise.
    ///     Same sharing semantics as GetDefault — multiple DurableShell instances
    ///     with the same workflow ID share counters, store, and occurrences.
    /// </summary>
    public static DurableWorkflowState GetShared(string workflowId) =>
        s_states.GetOrAdd(
            workflowId,
            id => {
                DurableWorkflowState state = new(CreateStore(id), id, "");
                AppDomain.CurrentDomain.ProcessExit += (_, _) => state.Dispose();
                return state;
            });

    /// <summary>Creates private state for explicit DurableShell construction (custom store/ID).</summary>
    public static DurableWorkflowState CreatePrivate(IDurabilityStore store, string workflowId, string baseSequencePath) =>
        new(store, workflowId, baseSequencePath);

    public int IncrementTotalCreated() => Interlocked.Increment(ref _totalCreated);

    public void IncrementPendingCount() => Interlocked.Increment(ref _pendingCount);

    public void OnWorkerCommitted() => Interlocked.Decrement(ref _pendingCount);

    public void OnWorkerAbandoned() => Interlocked.Decrement(ref _pendingCount);

    public (string key, int occurrence, string sequencePath) NextOccurrence(string scriptHash) {
        int occurrence = _hashOccurrences.AddOrUpdate(scriptHash, 1, (_, v) => v + 1);
        string key = $"{scriptHash}-{occurrence:D3}";
        string sequencePath = string.IsNullOrEmpty(BaseSequencePath) ? key : $"{BaseSequencePath}.{key}";
        return (key, occurrence, sequencePath);
    }

    public void Dispose() {
        if (Interlocked.Exchange(ref _disposed, 1) == 1) {
            return;
        }

        bool allSettled = Volatile.Read(ref _pendingCount) == 0 && Volatile.Read(ref _totalCreated) > 0;
        bool cleanExit = Environment.ExitCode == 0;

        if (allSettled && cleanExit) {
            Store.Clear();
        }

        Store.Dispose();
    }

    private static IDurabilityStore CreateStore(string workflowId) {
        if (GitHubActionsEnvironment.IsDetected()) {
            return new GitHubActionsCacheDurabilityStore(workflowId, GitHubActionsEnvironment.FromCurrent());
        }

        return new FileSystemDurabilityStore(Path.Combine(Path.GetTempPath(), "calq-cmd-cache", workflowId));
    }

    internal static string ComputeWorkflowId() {
        string input = Environment.CommandLine + "\0" + Environment.CurrentDirectory;
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash)[..16]
            .ToLowerInvariant();
    }
}
