namespace CalqFramework.Cmd.Shell;

public abstract class ShellBase : IShell {
    public IShellWorkerErrorHandler ErrorHandler { get; init; } = new ShellWorkerErrorHandler();
    public IShellCommandPostprocessor Postprocessor { get; init; } = new ShellCommandPostprocessor();

    public abstract ShellWorkerBase CreateShellWorker(ShellCommand shellCommand, CancellationToken cancellationToken = default);

    public abstract string MapToInternalPath(string hostPath);
}
