namespace CalqFramework.Cmd.Shell.SystemProcess;

/// <summary>
///     Process-based output stream that wraps a process's standard output for shell operations.
///     Handles process exit codes and input relay task monitoring.
/// </summary>
internal sealed class ProcessOutputStream(Process process, Task relayInputTask, IShellWorker shellWorker) : ShellWorkerOutputStream(shellWorker) {
    private readonly Stream _innerStream = process.StandardOutput.BaseStream;
    private readonly Process _process = process;
    private readonly Task _realyInputTask = relayInputTask;

    /// <summary>
    ///     The underlying process standard output stream.
    /// </summary>
    protected override Stream InnerStream => _innerStream;

    /// <summary>
    ///     Gets the process exit code after the process completes.
    ///     Waits for the process to exit before returning the error information.
    /// </summary>
    /// <returns>Error information containing the process exit code</returns>
    protected override Error GetError() {
        _process.WaitForExit();
        return new Error(_process.ExitCode, null);
    }

    /// <summary>
    ///     Asynchronously gets the process exit code after the process completes.
    ///     Waits for the process to exit before returning the error information.
    /// </summary>
    /// <returns>Task containing error information with the process exit code</returns>
    protected override async Task<Error> GetErrorAsync() {
        await _process.WaitForExitAsync();
        return new Error(_process.ExitCode, null);
    }

    /// <summary>
    ///     Attempts to read data from the process output stream, checking for input relay task failures.
    /// </summary>
    /// <returns>Number of bytes read from the process output</returns>
    protected override int TryRead(Span<byte> buffer) {
        if (_realyInputTask.IsFaulted) {
            throw _realyInputTask.Exception;
        }

        return _innerStream.Read(buffer);
    }

    /// <summary>
    ///     Asynchronously attempts to read data from the process output stream, checking for input relay task failures.
    /// </summary>
    /// <returns>Task containing the number of bytes read from the process output</returns>
    protected override async ValueTask<int> TryReadAsync(Memory<byte> buffer, CancellationToken cancellationToken) {
        if (_realyInputTask.IsFaulted) {
            throw _realyInputTask.Exception;
        }

        return await _innerStream.ReadAsync(buffer, cancellationToken);
    }
}
