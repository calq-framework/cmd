using CalqFramework.Cmd.Shell;

[assembly: InternalsVisibleTo("CalqFramework.Cmd.AspNetCore")]

namespace CalqFramework.Cmd.Shells;

/// <summary>
///     Local tool shell that delegates to factory-created underlying shell.
/// </summary>
public class LocalTool : ShellBase {
    private readonly IShell _underlyingShell;

    public LocalTool() => _underlyingShell = Factory.CreateLocalTool();

    internal static ILocalToolFactory Factory { get; set; } = new LocalToolFactory();

    public override IShellWorker CreateShellWorker(ShellScript shellScript, Stream? inputStream, bool disposeOnCompletion = true) =>
        _underlyingShell.CreateShellWorker(shellScript, inputStream, disposeOnCompletion);

    public override string MapToHostPath(string internalPath) => _underlyingShell.MapToHostPath(internalPath);

    public override string MapToInternalPath(string hostPath) => _underlyingShell.MapToInternalPath(hostPath);
}
