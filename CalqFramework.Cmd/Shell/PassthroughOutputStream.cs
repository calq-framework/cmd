namespace CalqFramework.Cmd.Shell;

/// <summary>
///     ShellWorkerOutputStream that reads from a provided stream without
///     transformation or error interpretation. Always reports success (error code 0).
/// </summary>
internal class PassthroughOutputStream(Stream stream, IShellWorker shellWorker) : ShellWorkerOutputStream(shellWorker) {
    private readonly Stream _stream = stream;

    protected override Stream InnerStream => _stream;

    protected override int TryRead(Span<byte> buffer) => _stream.Read(buffer);

    protected override ValueTask<int> TryReadAsync(Memory<byte> buffer, CancellationToken cancellationToken) =>
        _stream.ReadAsync(buffer, cancellationToken);

    protected override Error GetError() => new(0, null);

    protected override Task<Error> GetErrorAsync() => Task.FromResult(new Error(0, null));
}
