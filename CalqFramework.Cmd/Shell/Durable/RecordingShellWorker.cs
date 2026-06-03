namespace CalqFramework.Cmd.Shell.Durable;

/// <summary>
///     Extends ShellWorkerBase — gets free piping, lifecycle, EnsurePipeIsCompletedAsync.
///     Sets DurabilityContext in InitializeAsync (after ShellWorkerBase handles piping,
///     before inner worker's InitializeAsync sends HTTP) — correct by construction.
///     Creates inner worker lazily with stripped ShellScript to prevent double-piping.
///     Guards against null _recordingOutput in Dispose (cancellation before InitializeAsync).
/// </summary>
internal class RecordingShellWorker(IShell innerShell, ShellScript shellScript, Stream? inputStream, string key, string workflowId, string sequencePath, DurableWorkflowState state, bool disposeOnCompletion)
    : ShellWorkerBase(shellScript, inputStream, disposeOnCompletion) {
    private bool _disposed;

    private IShellWorker? _realWorker;
    private RecordingOutputStream? _recordingOutput;

    public override ShellWorkerOutputStream StandardOutput => _recordingOutput!;

    public override Task<string> ReadErrorMessageAsync(CancellationToken ct) =>
        _realWorker?.ReadErrorMessageAsync(ct) ?? Task.FromResult(string.Empty);

    protected override async Task InitializeAsync(ShellScript shellScript, CancellationToken ct) {
        // 1. Set DurabilityContext — piping already completed in ShellWorkerBase.StartAsync
        DurabilityContext.Current.Value = new DurableWorkflowContext(workflowId, sequencePath, state.Store);

        // 2. Create inner worker with stripped script (no PipedShellScript → no re-piping)
        ShellScript innerScript = new(innerShell, shellScript.Script) {
            WorkingDirectory = shellScript.WorkingDirectory
        };
        _realWorker = innerShell.CreateShellWorker(innerScript, InputStream, false);
        await _realWorker.StartAsync(ct);

        // 3. Wrap output for recording
        _recordingOutput = new RecordingOutputStream(_realWorker.StandardOutput, key, state.Store, this);
    }

    protected override void Dispose(bool disposing) {
        if (!_disposed) {
            _disposed = true;

            if (disposing) {
                if (_recordingOutput != null) {
                    if (_recordingOutput.ReachedEof && !_recordingOutput.ErrorObserved) {
                        _recordingOutput.CloseStoreStream();
                        state.Store.Commit(key);
                        state.OnWorkerCommitted();
                    } else if (!_recordingOutput.ErrorObserved) {
                        _recordingOutput.CloseStoreStream();
                        state.Store.Discard(key);
                        state.OnWorkerAbandoned();
                    } else {
                        _recordingOutput.CloseStoreStream();
                        // ErrorObserved: pendingCount stays elevated → prevents cleanup
                    }
                } else {
                    // InitializeAsync never completed (cancellation during piping).
                    // Store.Create was never called — nothing to commit/discard.
                    // Decrement pendingCount to avoid permanent elevation.
                    state.OnWorkerAbandoned();
                }

                _realWorker?.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}
