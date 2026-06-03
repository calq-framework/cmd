namespace CalqFramework.Cmd.Shell.Durable;

/// <summary>
///     Records bytes as they flow through to the store's writable stream.
///     Tracks ReachedEof and ErrorObserved state — lifecycle decisions made by
///     RecordingShellWorker.Dispose, not here.
///     No internal buffering — data flows directly to the store stream.
/// </summary>
internal class RecordingOutputStream(ShellWorkerOutputStream inner, string key, IDurabilityStore store, IShellWorker shellWorker) : ShellWorkerOutputStream(shellWorker) {
    private readonly ShellWorkerOutputStream _inner = inner;
    private readonly Stream _storeStream = store.Create(key);
    private bool _storeStreamClosed;

    public bool ReachedEof { get; private set; }

    public bool ErrorObserved { get; private set; }

    protected override Stream InnerStream => _inner;

    protected override int TryRead(Span<byte> buffer) {
        int read;
        try {
            read = _inner.Read(buffer);
        } catch {
            ErrorObserved = true;
            throw;
        }

        if (read > 0) {
            _storeStream.Write(buffer[..read]);
        } else {
            ReachedEof = true;
            _storeStream.Flush();
        }

        return read;
    }

    protected override async ValueTask<int> TryReadAsync(Memory<byte> buffer, CancellationToken ct) {
        int read;
        try {
            read = await _inner.ReadAsync(buffer, ct);
        } catch {
            ErrorObserved = true;
            throw;
        }

        if (read > 0) {
            await _storeStream.WriteAsync(buffer[..read], ct);
        } else {
            ReachedEof = true;
            await _storeStream.FlushAsync(ct);
        }

        return read;
    }

    protected override Error GetError() => new(0, null);

    protected override Task<Error> GetErrorAsync() => Task.FromResult(new Error(0, null));

    internal void CloseStoreStream() {
        if (!_storeStreamClosed) {
            _storeStreamClosed = true;
            _storeStream.Dispose();
        }
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            CloseStoreStream();
        }

        base.Dispose(disposing);
    }
}
