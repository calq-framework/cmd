namespace CalqFramework.Cmd.Shell;

public abstract class ShellBase : IShell {
    public IShellWorkerErrorHandler ErrorHandler { get; init; } = new ShellWorkerErrorHandler();
    public TextReader? In { get; init; } = Console.In;
    public IShellCommandPostprocessor Postprocessor { get; init; } = new ShellCommandPostprocessor();

    public ShellWorkerBase CreateShellWorker(ShellCommand shellCommand, CancellationToken cancellationToken = default) {
        return CreateShellWorker(shellCommand, this.In, cancellationToken);
    }

    public abstract ShellWorkerBase CreateShellWorker(ShellCommand shellCommand, TextReader? inputReader, CancellationToken cancellationToken = default);
    public abstract string MapToInternalPath(string hostPath);
}
