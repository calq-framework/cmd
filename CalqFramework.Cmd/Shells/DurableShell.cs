using CalqFramework.Cmd.Shell;
using CalqFramework.Cmd.Shell.Durable;

namespace CalqFramework.Cmd.Shells;

/// <summary>
///     Transparent IShell decorator. Intercepts CreateShellWorker to cache/replay.
///     Default shell for CLI — provides resume-from-failure for multi-step workflows.
///     Multiple instances sharing the same workflow ID share state via DurableWorkflowState.
///     Implements IDisposable for ASP.NET Core per-request lifecycle (private state only).
/// </summary>
public class DurableShell : ShellDecoratorBase, IDisposable {
    private static readonly ActivitySource s_activitySource = new("CalqFramework.Cmd.Durability");
    private readonly bool _ownsState;

    private readonly DurableWorkflowState _state;

    /// <summary>
    ///     CLI default: auto-detected store, optional explicit workflow ID.
    ///     Store is auto-selected: GitHub Actions Cache on CI, filesystem locally.
    /// </summary>
    public DurableShell(IShell inner, string? workflowId = null) : base(inner) {
        string id = workflowId ?? DurableWorkflowState.ComputeWorkflowId();
        _state = DurableWorkflowState.GetShared(id);
        _ownsState = false;
    }

    /// <summary>Full control: private state with injected store, explicit workflow ID, optional base sequence path.</summary>
    public DurableShell(IShell inner, IDurabilityStore store, string workflowId, string baseSequencePath = "") : base(inner) {
        _state = DurableWorkflowState.CreatePrivate(store, workflowId, baseSequencePath);
        _ownsState = true;
    }

    /// <summary>
    ///     Minimum time between commits. Steps completing within this interval
    ///     after the last commit skip caching (execute fresh on retry).
    ///     Default: TimeSpan.Zero (cache every step — correctness over throughput).
    ///     Set higher only for workflows with many cheap, idempotent steps where
    ///     per-step network cost dominates.
    /// </summary>
    public TimeSpan MinCommitInterval { get; init; } = TimeSpan.Zero;

    /// <summary>Discards all cached outputs for the specified workflow, forcing full re-execution on next run.</summary>
    public static void Clear(string workflowId) {
        string cacheDir = Path.Combine(Path.GetTempPath(), "calq-cmd-cache", workflowId);
        try {
            Directory.Delete(cacheDir, true);
        } catch (IOException) {
        }
    }

    /// <summary>Discards all workflow caches.</summary>
    public static void ClearAll() {
        string baseDir = Path.Combine(Path.GetTempPath(), "calq-cmd-cache");
        try {
            Directory.Delete(baseDir, true);
        } catch (IOException) {
        }
    }

    public void Dispose() {
        if (_ownsState) {
            _state.Dispose();
        }
    }

    public override IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true) {
        _state.IncrementTotalCreated();

        string scriptHash = ComputeScriptHash(shellScript);
        (string? key, int _, string? sequencePath) = _state.NextOccurrence(scriptHash);

        using Activity? activity = s_activitySource.StartActivity("DurableShell.CreateShellWorker");
        activity?.SetTag("calq.durability.workflow_id", _state.WorkflowId);
        activity?.SetTag("calq.durability.script_hash", scriptHash);
        activity?.SetTag("calq.durability.key", key);

        Stream? cachedStream = _state.Store.Get(key);
        if (cachedStream != null) {
            activity?.SetTag("calq.durability.cache_hit", true);
            return new NullShellWorker(shellScript, cachedStream, disposeOnCompletion);
        }

        activity?.SetTag("calq.durability.cache_hit", false);
        _state.IncrementPendingCount();

        return new RecordingShellWorker(Inner, shellScript, inputStream, key, _state.WorkflowId, sequencePath, _state, disposeOnCompletion);
    }

    private static string ComputeScriptHash(ShellScript shellScript) {
        if (shellScript.PipedShellScript == null) {
            string input = shellScript.Script + "\0" + shellScript.WorkingDirectory;
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash)[..16]
                .ToLowerInvariant();
        }

        string pipedHash = ComputeScriptHash(shellScript.PipedShellScript);
        string combined = shellScript.Script + "\0" + shellScript.WorkingDirectory + "\0" + pipedHash;
        byte[] combinedHash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(combinedHash)[..16]
            .ToLowerInvariant();
    }
}
