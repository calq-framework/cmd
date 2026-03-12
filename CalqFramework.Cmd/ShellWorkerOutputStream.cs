namespace CalqFramework.Cmd;

/// <summary>
///     Stream wrapper for shell worker output that handles automatic disposal and error detection.
///     Disposes the underlying worker when DisposeOnCompletion is enabled and reading completes.
/// </summary>
/// <remarks>
///     Creates a new shell worker output stream wrapper.
/// </remarks>
// INFO throwing with only ErrorCode is OK, stderr might contain diagnostics/info instead of error errorMessage so don't throw just because not empty
public abstract class ShellWorkerOutputStream(IShellWorker shellWorker) : Stream {
    private readonly IShellWorker _shellWorker = shellWorker;

    public override bool CanRead => InnerStream.CanRead;

    public override bool CanSeek => InnerStream.CanSeek;

    public override bool CanWrite => InnerStream.CanWrite;

    public override long Length => InnerStream.Length;

    public override long Position {
        get => InnerStream.Position;
        set => InnerStream.Position = value;
    }

    protected abstract Stream InnerStream { get; }

    public override void Flush() => InnerStream.Flush();

    public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

    public override int Read(Span<byte> buffer) {
        int bytesRead;
        try {
            bytesRead = TryRead(buffer);
        } catch (OperationCanceledException) {
            throw;
        } catch (Exception ex) {
            throw new ShellWorkerException(null, $"Error code: {null}", ex);
        }

        if (bytesRead == 0) {
            Error error = GetError();

            if (error.ErrorCode != 0) {
                throw new ShellWorkerException(error.ErrorCode, $"Error code: {error.ErrorCode}", error.Exception);
            }

            if (_shellWorker.DisposeOnCompletion) {
                _shellWorker.Dispose();
            }
        }

        return bytesRead;
    }

    public override async ValueTask<int>
        ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {
        int bytesRead;
        try {
            bytesRead = await TryReadAsync(buffer, cancellationToken);
        } catch (OperationCanceledException) {
            throw;
        } catch (Exception ex) {
            throw new ShellWorkerException(null, $"Error code: {null}", ex);
        }

        if (bytesRead == 0) {
            Error error = await GetErrorAsync();

            if (error.ErrorCode != 0) {
                throw new ShellWorkerException(error.ErrorCode, $"Error code: {error.ErrorCode}", error.Exception);
            }

            if (_shellWorker.DisposeOnCompletion) {
                _shellWorker.Dispose();
            }
        }

        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin) => InnerStream.Seek(offset, origin);

    public override void SetLength(long value) => InnerStream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count) => InnerStream.Write(buffer, offset, count);

    /// <summary>
    ///     Disposes the stream and optionally the underlying worker when DisposeOnCompletion is enabled.
    /// </summary>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            InnerStream.Dispose();

            if (_shellWorker.DisposeOnCompletion) {
                _shellWorker.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    protected abstract Error GetError();

    protected abstract Task<Error> GetErrorAsync();

    protected abstract int TryRead(Span<byte> buffer);

    protected abstract ValueTask<int> TryReadAsync(Memory<byte> buffer, CancellationToken cancellationToken);

    /// <summary>
    ///     Error information with optional error code and exception.
    /// </summary>
    protected readonly record struct Error(long? ErrorCode, Exception? Exception);
}
